using AspNetCore.WebAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProfileController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET api/profile/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _db.Users
                .Where(u => u.Id == id)
                .Select(u => new { u.Id, u.Username, u.Photo, u.Role })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound(new { message = "Користувача не знайдено" });

            var library = await _db.UserGames
                .Where(ug => ug.UserId == id)
                .Include(ug => ug.Game)
                .Select(ug => new { ug.Game.Id, ug.Game.Name, ug.Game.Photo, ug.Game.GPA, ug.PurchasedAt })
                .ToListAsync();

            var screenshots = await _db.Screenshots
                .Where(s => s.UserId == id)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new { s.Id, s.FileName, s.Caption, s.Likes, s.CreatedAt })
                .Take(12)
                .ToListAsync();

            return Ok(new { user, library, screenshots });
        }

        // GET api/profile/me — свой профиль с балансом
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _db.Users
                .Where(u => u.Id == myId)
                .Select(u => new { u.Id, u.Username, u.Photo, u.Role, u.Balance })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            var library = await _db.UserGames
                .Where(ug => ug.UserId == myId)
                .Include(ug => ug.Game)
                .Select(ug => new { ug.Game.Id, ug.Game.Name, ug.Game.Photo, ug.Game.GPA, ug.PurchasedAt })
                .ToListAsync();

            var screenshots = await _db.Screenshots
                .Where(s => s.UserId == myId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new { s.Id, s.FileName, s.Caption, s.Likes, s.CreatedAt })
                .Take(12)
                .ToListAsync();

            return Ok(new { user, library, screenshots });
        }

        // PUT api/profile/me — редактировать свой профиль
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(myId);
            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var taken = await _db.Users.AnyAsync(u => u.Username == request.Username && u.Id != myId);
                if (taken) return BadRequest(new { message = "Це ім'я вже зайнято" });
                user.Username = request.Username.Trim();
            }

            await _db.SaveChangesAsync();
            return Ok(new { user.Id, user.Username, user.Photo, user.Role, user.Balance });
        }

        // POST api/profile/me/avatar — загрузить аватар
        [HttpPost("me/avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не вибрано" });

            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(myId);
            if (user == null) return NotFound();

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"avatar_{myId}{ext}";
            var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "avatars");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            user.Photo = fileName;
            await _db.SaveChangesAsync();

            return Ok(new { fileName });
        }
    }

    public class UpdateProfileRequest
    {
        public string? Username { get; set; }
    }
}