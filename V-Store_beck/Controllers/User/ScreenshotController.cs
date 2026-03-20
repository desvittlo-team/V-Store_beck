using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/screenshots")]
    public class ScreenshotController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly string _folder;

        public ScreenshotController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            var root = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _folder = Path.Combine(root, "screenshots");
            Directory.CreateDirectory(_folder);
        }

        // GET api/screenshots
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var screenshots = await _db.Screenshots
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new {
                    s.Id,
                    s.FileName,
                    s.Caption,
                    s.Likes,
                    s.CreatedAt,
                    s.UserId,
                    Username = s.User.Username
                })
                .ToListAsync();
            return Ok(screenshots);
        }

        // POST api/screenshots/upload
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string? caption)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не вибрано" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{ext}";
            var path = Path.Combine(_folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            var screenshot = new Screenshot
            {
                UserId = userId,
                FileName = fileName,
                Caption = caption
            };
            _db.Screenshots.Add(screenshot);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                screenshot.Id,
                screenshot.FileName,
                screenshot.Caption,
                screenshot.Likes,
                screenshot.CreatedAt,
                screenshot.UserId
            });
        }

        // POST api/screenshots/{id}/like
        [HttpPost("{id}/like")]
        [Authorize]
        public async Task<IActionResult> Like(int id)
        {
            var screenshot = await _db.Screenshots.FindAsync(id);
            if (screenshot == null) return NotFound();
            screenshot.Likes++;
            await _db.SaveChangesAsync();
            return Ok(new { screenshot.Likes });
        }

        // DELETE api/screenshots/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var screenshot = await _db.Screenshots.FindAsync(id);
            if (screenshot == null) return NotFound();
            if (screenshot.UserId != userId) return Forbid();

            var path = Path.Combine(_folder, screenshot.FileName);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            _db.Screenshots.Remove(screenshot);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Видалено" });
        }
    }
}