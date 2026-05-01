using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/showcases")]
    public class ShowcaseController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ShowcaseController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET api/showcases/{userId} — витрины пользователя
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserShowcases(int userId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var showcases = await _db.Showcases
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Position)
                .Include(s => s.Items)
                    .ThenInclude(si => si.InventoryItem)
                        .ThenInclude(ii => ii!.Item)
                .Include(s => s.Items)
                    .ThenInclude(si => si.Screenshot)
                .Include(s => s.Items)
                    .ThenInclude(si => si.UserGame)
                        .ThenInclude(ug => ug!.Game)
                .ToListAsync();

            var result = showcases.Select(s => new
            {
                s.Id,
                s.Type,
                s.Title,
                s.Position,
                Items = s.Items.OrderBy(i => i.Position).Select(si => new
                {
                    si.Id,
                    si.Position,
                    si.IllustrationUrl,
                    InventoryItem = si.InventoryItem == null ? null : new
                    {
                        si.InventoryItem.Id,
                        si.InventoryItem.Item.Name,
                        si.InventoryItem.Item.ItemType,
                        PhotoUrl = $"{baseUrl}/items/{si.InventoryItem.Item.Photo}"
                    },
                    Screenshot = si.Screenshot == null ? null : new
                    {
                        si.Screenshot.Id,
                        si.Screenshot.Caption,
                        Url = $"{baseUrl}/screenshots/{si.Screenshot.FileName}"
                    },
                    Game = si.UserGame == null ? null : new
                    {
                        si.UserGame.Game.Id,
                        si.UserGame.Game.Name,
                        si.UserGame.Game.GPA,
                        PhotoUrl = $"{baseUrl}/images/{si.UserGame.Game.Photo}"
                    }
                })
            });

            return Ok(result);
        }

        // GET api/showcases/my — мои витрины
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyShowcases()
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return await GetUserShowcases(myId);
        }

        // POST api/showcases — создать витрину
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateShowcase([FromBody] CreateShowcaseRequest request)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var validTypes = new[] { "illustration", "screenshots", "inventory", "games" };
            if (!validTypes.Contains(request.Type))
                return BadRequest(new { message = "Невірний тип вітрини" });

            var count = await _db.Showcases.CountAsync(s => s.UserId == myId);
            if (count >= 6)
                return BadRequest(new { message = "Максимум 6 вітрин" });

            var showcase = new Showcase
            {
                UserId = myId,
                Type = request.Type,
                Title = request.Title.Trim(),
                Position = count
            };

            _db.Showcases.Add(showcase);
            await _db.SaveChangesAsync();

            return Ok(new { showcase.Id, showcase.Type, showcase.Title, showcase.Position, Items = new List<object>() });
        }

        // DELETE api/showcases/{id} — удалить витрину
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteShowcase(int id)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var showcase = await _db.Showcases.FirstOrDefaultAsync(s => s.Id == id && s.UserId == myId);
            if (showcase == null) return NotFound();

            _db.Showcases.Remove(showcase);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Вітрину видалено" });
        }

        // PUT api/showcases/{id}/title — переименовать витрину
        [HttpPut("{id}/title")]
        [Authorize]
        public async Task<IActionResult> UpdateTitle(int id, [FromBody] UpdateTitleRequest request)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var showcase = await _db.Showcases.FirstOrDefaultAsync(s => s.Id == id && s.UserId == myId);
            if (showcase == null) return NotFound();

            showcase.Title = request.Title.Trim();
            await _db.SaveChangesAsync();

            return Ok(new { showcase.Id, showcase.Title });
        }

        // POST api/showcases/{id}/items — добавить item в витрину
        [HttpPost("{id}/items")]
        [Authorize]
        public async Task<IActionResult> AddItem(int id, [FromBody] AddShowcaseItemRequest request)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var showcase = await _db.Showcases
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == myId);

            if (showcase == null) return NotFound();

            var maxItems = showcase.Type == "illustration" ? 1 : 5;
            if (showcase.Items.Count >= maxItems)
                return BadRequest(new { message = $"Максимум {maxItems} елементів у цій вітрині" });

            var newItem = new ShowcaseItem
            {
                ShowcaseId = id,
                Position = showcase.Items.Count
            };

            switch (showcase.Type)
            {
                case "inventory":
                    if (!request.InventoryItemId.HasValue)
                        return BadRequest(new { message = "Вкажіть InventoryItemId" });
                    var invItem = await _db.InventoryItems
                        .Include(ii => ii.Item)
                        .FirstOrDefaultAsync(ii => ii.Id == request.InventoryItemId.Value && ii.UserId == myId);
                    if (invItem == null) return NotFound(new { message = "Предмет не знайдено в інвентарі" });
                    newItem.InventoryItemId = invItem.Id;
                    break;

                case "screenshots":
                    if (!request.ScreenshotId.HasValue)
                        return BadRequest(new { message = "Вкажіть ScreenshotId" });
                    var screenshot = await _db.Screenshots
                        .FirstOrDefaultAsync(s => s.Id == request.ScreenshotId.Value && s.UserId == myId);
                    if (screenshot == null) return NotFound(new { message = "Скріншот не знайдено" });
                    newItem.ScreenshotId = screenshot.Id;
                    break;

                case "games":
                    if (!request.UserGameId.HasValue)
                        return BadRequest(new { message = "Вкажіть UserGameId" });
                    var userGame = await _db.UserGames
                        .Include(ug => ug.Game)
                        .FirstOrDefaultAsync(ug => ug.Id == request.UserGameId.Value && ug.UserId == myId);
                    if (userGame == null) return NotFound(new { message = "Гру не знайдено в бібліотеці" });
                    newItem.UserGameId = userGame.Id;
                    break;

                case "illustration":
                    if (string.IsNullOrWhiteSpace(request.IllustrationUrl))
                        return BadRequest(new { message = "Вкажіть IllustrationUrl" });
                    newItem.IllustrationUrl = request.IllustrationUrl;
                    break;
            }

            _db.ShowcaseItems.Add(newItem);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Додано до вітрини", newItem.Id });
        }

        // POST api/showcases/{id}/illustration — загрузить иллюстрацию
        [HttpPost("{id}/illustration")]
        [Authorize]
        public async Task<IActionResult> UploadIllustration(int id, IFormFile file)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var showcase = await _db.Showcases
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == myId && s.Type == "illustration");

            if (showcase == null) return NotFound();

            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowed.Contains(file.ContentType))
                return BadRequest(new { message = "Формат не підтримується" });

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "Максимум 10MB" });

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"illustration_{myId}_{DateTime.UtcNow.Ticks}{ext}";
            var folder = Path.Combine(
                _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "illustrations");
            Directory.CreateDirectory(folder);

            await using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}/illustrations/{fileName}";

            // удаляем старую иллюстрацию если есть
            var existing = showcase.Items.FirstOrDefault();
            if (existing != null)
            {
                _db.ShowcaseItems.Remove(existing);
                await _db.SaveChangesAsync();
            }

            var newItem = new ShowcaseItem
            {
                ShowcaseId = id,
                IllustrationUrl = url,
                Position = 0
            };

            _db.ShowcaseItems.Add(newItem);
            await _db.SaveChangesAsync();

            return Ok(new { url, newItem.Id });
        }

        // DELETE api/showcases/items/{itemId} — удалить item из витрины
        [HttpDelete("items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var item = await _db.ShowcaseItems
                .Include(si => si.Showcase)
                .FirstOrDefaultAsync(si => si.Id == itemId && si.Showcase.UserId == myId);

            if (item == null) return NotFound();

            _db.ShowcaseItems.Remove(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Видалено" });
        }

        // PUT api/showcases/reorder — изменить порядок витрин
        [HttpPut("reorder")]
        [Authorize]
        public async Task<IActionResult> ReorderShowcases([FromBody] List<ReorderItem> order)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var showcases = await _db.Showcases.Where(s => s.UserId == myId).ToListAsync();

            foreach (var s in showcases)
            {
                var match = order.FirstOrDefault(o => o.Id == s.Id);
                if (match != null) s.Position = match.Position;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Порядок збережено" });
        }
    }

    public class CreateShowcaseRequest
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class AddShowcaseItemRequest
    {
        public int? InventoryItemId { get; set; }
        public int? ScreenshotId { get; set; }
        public int? UserGameId { get; set; }
        public string? IllustrationUrl { get; set; }
    }

    public class UpdateTitleRequest
    {
        public string Title { get; set; } = string.Empty;
    }

    public class ReorderItem
    {
        public int Id { get; set; }
        public int Position { get; set; }
    }
}