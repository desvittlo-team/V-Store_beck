using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/globalchat")]
    public class GlobalChatController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public GlobalChatController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET api/globalchat — останні 50 повідомлень
        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _db.GlobalMessages
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .Select(m => new
                {
                    m.Id,
                    m.Text,
                    m.CreatedAt,
                    m.UserId,
                    Username = m.User.Username
                })
                .ToListAsync();

            return Ok(messages.OrderBy(m => m.CreatedAt));
        }

        // POST api/globalchat
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] GlobalMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 500)
                return BadRequest(new { message = "Текст від 1 до 500 символів" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var message = new GlobalMessage
            {
                UserId = userId,
                Text = request.Text.Trim()
            };

            _db.GlobalMessages.Add(message);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message.Id,
                message.Text,
                message.CreatedAt,
                message.UserId,
                Username = user.Username
            });
        }
        [HttpPost("image")]
        [Authorize]
        public async Task<IActionResult> SendImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не вибрано" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"global_{userId}_{DateTime.UtcNow.Ticks}{ext}";
            var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "chat-images");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            var message = new GlobalMessage
            {
                UserId = userId,
                Text = "📷 Фото",
                Type = "image",
                ImageFileName = fileName
            };

            _db.GlobalMessages.Add(message);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message.Id,
                message.Text,
                message.Type,
                message.ImageFileName,
                message.CreatedAt,
                message.UserId,
                Username = user.Username
            });
        }
    }

    public class GlobalMessageRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}
