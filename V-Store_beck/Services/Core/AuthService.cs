using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;

namespace AspNetCore.WebAPI.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        private const string DefaultUserRole = "User";

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // =========================са
        // REGISTER
        // =========================
        public async Task<User> Register(string username, string email, string password)
        {
            // нормализуем email
            email = email.ToLower().Trim();

            // один запрос вместо двух
            bool exists = await _context.Users
                .AnyAsync(u => u.Email == email || u.Username == username);

            if (exists)
                throw new InvalidOperationException("User with this email or username already exists");

            var user = new User
            {   

                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = DefaultUserRole
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // =========================
        // LOGIN
        // =========================
        public async Task<User?> Login(string email, string password)
        {
            email = email.ToLower().Trim();

            var user = await _context.Users
                .AsNoTracking() // оптимизация (не нужно отслеживание EF)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            bool validPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!validPassword)
                return null;

            return user;
        }

        // =========================
        // JWT GENERATION
        // =========================
        public string GenerateJwt(User user)
        {
            string jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key not configured");

            string issuer = _config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer not configured");

            string audience = _config["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience not configured");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // =========================
        // ADMIN PASSWORD RESET (DEV)
        // =========================
        public async Task<User?> FixAdminPassword()
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == "admin");

            if (user == null)
                return null;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");

            await _context.SaveChangesAsync();

            return user;
        }
        public class AuthResult
        {
            public int Id { get; set; }  // <- добавь
            public string Token { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}