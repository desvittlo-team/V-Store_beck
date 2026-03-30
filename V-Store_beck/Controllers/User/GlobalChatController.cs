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

        public GlobalChatController(AppDbContext db)
        {
            _db = db;
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
    }

    public class GlobalMessageRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}
