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
        private readonly IWebHostEnvironment _env;  
        public ChatController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;

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
                        SenderId = m.SenderId,
                        Type = m.Type
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
                .Include(m => m.InventoryItem)
                    .ThenInclude(ii => ii != null ? ii.Item : null)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Text = m.Text,
                    Type = m.Type,
                    Amount = m.Amount,
                    CreatedAt = m.CreatedAt,
                    IsRead = m.IsRead,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Item = m.InventoryItem != null ? new ItemPreviewDto
                    {
                        Id = m.InventoryItem.Item.Id,
                        Name = m.InventoryItem.Item.Name,
                        Photo = m.InventoryItem.Item.Photo
                    } : null
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

        // POST api/chat/messages — текстовое сообщение
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
                Text = request.Text.Trim(),
                Type = "text"
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            return Ok(new MessageDto
            {
                Id = message.Id,
                Text = message.Text,
                Type = message.Type,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId
            });
        }

        // POST api/chat/send-money — отправить деньги
        [HttpPost("send-money")]
        public async Task<IActionResult> SendMoney([FromBody] SendMoneyRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest(new { message = "Сума повинна бути більше 0" });

            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (myId == request.ReceiverId)
                return BadRequest(new { message = "Не можна відправити собі" });

            var sender = await _db.Users.FindAsync(myId);
            var receiver = await _db.Users.FindAsync(request.ReceiverId);

            if (sender == null || receiver == null)
                return NotFound(new { message = "Користувача не знайдено" });

            if (sender.Balance < request.Amount)
                return BadRequest(new { message = "Недостатньо коштів" });

            sender.Balance -= request.Amount;
            receiver.Balance += request.Amount;

            var message = new Message
            {
                SenderId = myId,
                ReceiverId = request.ReceiverId,
                Text = $"💰 Відправлено {request.Amount}$",
                Type = "money",
                Amount = request.Amount
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            return Ok(new MessageDto
            {
                Id = message.Id,
                Text = message.Text,
                Type = message.Type,
                Amount = message.Amount,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId
            });
        }

        // POST api/chat/send-item — отправить предмет из инвентаря
        [HttpPost("send-item")]
        public async Task<IActionResult> SendItem([FromBody] SendItemRequest request)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (myId == request.ReceiverId)
                return BadRequest(new { message = "Не можна відправити собі" });

            var invItem = await _db.InventoryItems
                .Include(ii => ii.Item)
                .FirstOrDefaultAsync(ii => ii.Id == request.InventoryItemId && ii.UserId == myId);

            if (invItem == null)
                return NotFound(new { message = "Предмет не знайдено в інвентарі" });

            var receiverExists = await _db.Users.AnyAsync(u => u.Id == request.ReceiverId);
            if (!receiverExists)
                return NotFound(new { message = "Отримувача не знайдено" });

            // меняем владельца предмета
            invItem.UserId = request.ReceiverId;

            var message = new Message
            {
                SenderId = myId,
                ReceiverId = request.ReceiverId,
                Text = $"🎁 Відправлено предмет: {invItem.Item.Name}",
                Type = "item",
                ItemId = invItem.Id
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            return Ok(new MessageDto
            {
                Id = message.Id,
                Text = message.Text,
                Type = message.Type,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Item = new ItemPreviewDto
                {
                    Id = invItem.Item.Id,
                    Name = invItem.Item.Name,
                    Photo = invItem.Item.Photo
                }
            });
        }
        [HttpPost("send-image")]
        public async Task<IActionResult> SendImage([FromForm] int receiverId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не вибрано" });

            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (myId == receiverId)
                return BadRequest(new { message = "Не можна відправити собі" });

            var receiverExists = await _db.Users.AnyAsync(u => u.Id == receiverId);
            if (!receiverExists)
                return NotFound(new { message = "Отримувача не знайдено" });

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"msg_{myId}_{DateTime.UtcNow.Ticks}{ext}";
            var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "chat-images");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            var message = new Message
            {
                SenderId = myId,
                ReceiverId = receiverId,
                Text = "📷 Фото",
                Type = "image",
                ImageFileName = fileName
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            return Ok(new MessageDto
            {
                Id = message.Id,
                Text = message.Text,
                Type = message.Type,
                ImageFileName = message.ImageFileName,
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
        public string Type { get; set; } = "text";
    }

    public class ConversationDto
    {
        public UserDto Partner { get; set; } = null!;
        public LastMessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ItemPreviewDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = "text";
        public decimal? Amount { get; set; }
        public string? ImageFileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public ItemPreviewDto? Item { get; set; }
    }

    public class SendMessageRequest
    {
        public int ReceiverId { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class SendMoneyRequest
    {
        public int ReceiverId { get; set; }
        public decimal Amount { get; set; }
    }

    public class SendItemRequest
    {
        public int ReceiverId { get; set; }
        public int InventoryItemId { get; set; }
    }
}