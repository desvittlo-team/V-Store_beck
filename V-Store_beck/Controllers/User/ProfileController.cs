using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _db.Users
                .Where(u => u.Id == id)
                .Select(u => new { u.Id, u.Username, u.Photo, u.Role })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound(new { message = "Користувача не знайдено" });

            var profile = await _db.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == id);

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

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                user = new
                {
                    user.Id,
                    user.Username,
                    Photo = user.Photo,
                    PhotoUrl = string.IsNullOrEmpty(user.Photo) ? null : $"{baseUrl}/avatars/{user.Photo}",
                    user.Role,
                    Bio = profile?.Bio,
                    BackgroundUrl = profile?.BackgroundUrl,
                    HideComments = profile?.HideComments ?? false,
                    PrivateLibrary = profile?.PrivateLibrary ?? false,
                    ShowOnline = profile?.ShowOnline ?? true,
                },
                library = library.Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Photo,
                    PhotoUrl = string.IsNullOrEmpty(g.Photo) ? null : $"{baseUrl}/items/{g.Photo}",
                    g.GPA,
                    g.PurchasedAt
                }),
                screenshots = screenshots.Select(s => new
                {
                    s.Id,
                    s.FileName,
                    Url = string.IsNullOrEmpty(s.FileName) ? null : $"{baseUrl}/screenshots/{s.FileName}",
                    s.Caption,
                    s.Likes,
                    s.CreatedAt
                })
            });
        }

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

            var profile = await _db.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == myId);

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

            return Ok(new
            {
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Photo,
                    user.Role,
                    user.Balance,
                    Bio = profile?.Bio,
                    BackgroundUrl = profile?.BackgroundUrl,
                    HideComments = profile?.HideComments ?? false,
                    PrivateLibrary = profile?.PrivateLibrary ?? false,
                    ShowOnline = profile?.ShowOnline ?? true,
                },
                library,
                screenshots
            });
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(myId);
            if (user == null) return NotFound();

            // Username живёт в User
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var taken = await _db.Users.AnyAsync(u => u.Username == request.Username && u.Id != myId);
                if (taken) return BadRequest(new { message = "Це ім'я вже зайнято" });
                user.Username = request.Username.Trim();
            }

            // Avatar file — уже через POST /avatar, но и из инвентаря
            if (request.AvatarInventoryItemId.HasValue)
            {
                var invItem = await _db.InventoryItems
                    .Include(i => i.Item)
                    .FirstOrDefaultAsync(i => i.Id == request.AvatarInventoryItemId.Value && i.UserId == myId);
                if (invItem != null)
                    user.Photo = invItem.Item.Photo;
            }

            await _db.SaveChangesAsync();

            // Всё остальное — в UserProfile
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == myId);
            if (profile == null)
            {
                profile = new UserProfile { UserId = myId };
                _db.UserProfiles.Add(profile);
            }

            if (request.Bio != null)
                profile.Bio = request.Bio;

            if (request.BackgroundInventoryItemId.HasValue)
            {
                var invItem = await _db.InventoryItems
                    .Include(i => i.Item)
                    .FirstOrDefaultAsync(i => i.Id == request.BackgroundInventoryItemId.Value && i.UserId == myId);
                if (invItem != null)
                    profile.BackgroundUrl = $"{Request.Scheme}://{Request.Host}/items/{invItem.Item.Photo}";
            }

            if (request.HideComments.HasValue) profile.HideComments = request.HideComments.Value;
            if (request.PrivateLibrary.HasValue) profile.PrivateLibrary = request.PrivateLibrary.Value;
            if (request.ShowOnline.HasValue) profile.ShowOnline = request.ShowOnline.Value;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Photo,
                user.Role,
                user.Balance,
                Bio = profile.Bio,
                BackgroundUrl = profile.BackgroundUrl,
                HideComments = profile.HideComments,
                PrivateLibrary = profile.PrivateLibrary,
                ShowOnline = profile.ShowOnline,
            });
        }

        [HttpPost("me/avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не вибрано" });

            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowed.Contains(file.ContentType))
                return BadRequest(new { message = "Формат не підтримується. Дозволено: jpg, png, webp, gif" });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Максимальний розмір — 5MB" });

            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(myId);
            if (user == null) return NotFound();

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"avatar_{myId}{ext}";
            var folder = Path.Combine(
                _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "avatars");
            Directory.CreateDirectory(folder);

            await using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);

            user.Photo = fileName;
            await _db.SaveChangesAsync();

            return Ok(new { fileName });
        }
    }

    public class UpdateProfileRequest
    {
        public string? Username { get; set; }
        public string? Bio { get; set; }
        public int? AvatarInventoryItemId { get; set; }
        public int? BackgroundInventoryItemId { get; set; }
        public bool? HideComments { get; set; }
        public bool? PrivateLibrary { get; set; }
        public bool? ShowOnline { get; set; }
    }
}