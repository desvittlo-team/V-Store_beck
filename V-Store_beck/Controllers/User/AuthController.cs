// Controllers/User/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using AspNetCore.WebAPI.Models;
using AspNetCore.WebAPI.Services;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.Register(request.Username, request.Email, request.Password);
                var token = _authService.GenerateJwt(user);

                return Ok(new AuthResult
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authService.Login(request.Email, request.Password);

            if (user is null)
                return Unauthorized(new { message = "Invalid email or password" });

            var token = _authService.GenerateJwt(user);

            return Ok(new AuthResult
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            });
        }
        [HttpGet("fix-admin")]
        public async Task<IActionResult> FixAdmin()
        {
            var user = await _authService.FixAdminPassword();
            return Ok(new { message = "Done", hash = user.PasswordHash });
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResult
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}