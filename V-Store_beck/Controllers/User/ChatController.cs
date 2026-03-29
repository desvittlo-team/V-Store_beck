using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ChatController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return BadRequest(new { message = "Мінімум 2 символи" });

            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var users = await _db.Users
                .Where(u => u.Username.Contains(q) && u.Id != myId)
                .Select(u => new { u.Id, u.Username, u.Photo })
                .Take(10)
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var partnerIds = await _db.Messages
                .Where(m => m.SenderId == myId || m.ReceiverId == myId)
                .Select(m => m.SenderId == myId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var result = new List<ConversationDto>();

            foreach (var partnerId in partnerIds)
            {
                var partner = await _db.Users
                    .Where(u => u.Id == partnerId)
                    .Select(u => new UserDto { Id = u.Id, Username = u.Username, Photo = u.Photo })
                    .FirstOrDefaultAsync();

                if (partner == null) continue;

                var lastMessage = await _db.Messages
                    .Where(m =>
                        (m.SenderId == myId && m.ReceiverId == partnerId) ||
                        (m.SenderId == partnerId && m.ReceiverId == myId))
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new LastMessageDto
                    {
                        Text = m.Text,
                        CreatedAt = m.CreatedAt,
                        SenderId = m.SenderId
                    })
                    .FirstOrDefaultAsync();

                var unreadCount = await _db.Messages
                    .CountAsync(m => m.SenderId == partnerId && m.ReceiverId == myId && !m.IsRead);

                result.Add(new ConversationDto
                {
                    Partner = partner,
                    LastMessage = lastMessage,
                    UnreadCount = unreadCount
                });
            }

            return Ok(result.OrderByDescending(c => c.LastMessage?.CreatedAt ?? DateTime.MinValue));
        }

        [HttpGet("messages/{partnerId}")]
        public async Task<IActionResult> GetMessages(int partnerId)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var partnerExists = await _db.Users.AnyAsync(u => u.Id == partnerId);
            if (!partnerExists)
                return NotFound(new { message = "Користувача не знайдено" });

            var messages = await _db.Messages
                .Where(m =>
                    (m.SenderId == myId && m.ReceiverId == partnerId) ||
                    (m.SenderId == partnerId && m.ReceiverId == myId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Text = m.Text,
                    CreatedAt = m.CreatedAt,
                    IsRead = m.IsRead,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId
                })
                .ToListAsync();

            var unread = await _db.Messages
                .Where(m => m.SenderId == partnerId && m.ReceiverId == myId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unread)
                msg.IsRead = true;

            if (unread.Any())
                await _db.SaveChangesAsync();

            return Ok(messages);
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 1000)
                return BadRequest(new { message = "Текст від 1 до 1000 символів" });

            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (myId == request.ReceiverId)
                return BadRequest(new { message = "Не можна писати собі" });

            var receiverExists = await _db.Users.AnyAsync(u => u.Id == request.ReceiverId);
            if (!receiverExists)
                return NotFound(new { message = "Отримувача не знайдено" });

            var message = new Message
            {
                SenderId = myId,
                ReceiverId = request.ReceiverId,
                Text = request.Text.Trim()
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            return Ok(new MessageDto
            {
                Id = message.Id,
                Text = message.Text,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId
            });
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
    }

    public class LastMessageDto
    {
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int SenderId { get; set; }
    }

    public class ConversationDto
    {
        public UserDto Partner { get; set; } = null!;
        public LastMessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
    }

    public class SendMessageRequest
    {
        public int ReceiverId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}