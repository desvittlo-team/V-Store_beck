using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/market")]
    public class MarketController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public MarketController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET api/market — все предметы
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Items
                .Where(i => !i.IsSold)
                .Include(i => i.Game)
                .Include(i => i.Seller)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new {
                    i.Id,
                    i.Name,
                    i.Description,
                    i.Photo,
                    i.Price,
                    i.CreatedAt,
                    Game = new { i.Game.Id, i.Game.Name },
                    Seller = new { i.Seller.Id, i.Seller.Username }
                })
                .ToListAsync();

            return Ok(items);
        }

        // POST api/market — выставить предмет
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateListing([FromForm] CreateItemRequest request, IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Price <= 0)
                return BadRequest(new { message = "Назва та ціна обов'язкові" });

            var gameExists = await _db.Game.AnyAsync(g => g.Id == request.GameId);
            if (!gameExists) return NotFound(new { message = "Гру не знайдено" });

            var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            string photoName = "default_item.png";
            if (file != null && file.Length > 0)
            {
                var ext = Path.GetExtension(file.FileName);
                photoName = $"item_{sellerId}_{DateTime.UtcNow.Ticks}{ext}";
                var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "items");
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, photoName);
                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
            }

            var item = new Item
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Photo = photoName,
                Price = request.Price,
                GameId = request.GameId,
                SellerId = sellerId
            };

            _db.Items.Add(item);
            await _db.SaveChangesAsync();

            return Ok(new { item.Id, item.Name, item.Price, item.Photo });
        }

        // POST api/market/{id}/buy — купить предмет
        [HttpPost("{id}/buy")]
        [Authorize]
        public async Task<IActionResult> Buy(int id)
        {
            var buyerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var item = await _db.Items
                .Include(i => i.Seller)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound(new { message = "Предмет не знайдено" });
            if (item.IsSold) return BadRequest(new { message = "Предмет вже продано" });
            if (item.SellerId == buyerId) return BadRequest(new { message = "Не можна купити свій предмет" });

            var buyer = await _db.Users.FindAsync(buyerId);
            if (buyer == null) return Unauthorized();

            if (buyer.Balance < item.Price)
                return BadRequest(new { message = "Недостатньо коштів" });

            // списываем деньги у покупателя
            buyer.Balance -= item.Price;

            // добавляем деньги продавцу
            var seller = await _db.Users.FindAsync(item.SellerId);
            if (seller != null) seller.Balance += item.Price;

            // помечаем как проданный
            item.IsSold = true;

            // добавляем в инвентарь покупателя
            _db.InventoryItems.Add(new InventoryItem
            {
                UserId = buyerId,
                ItemId = item.Id
            });

            await _db.SaveChangesAsync();

            return Ok(new { message = "Куплено!", balance = buyer.Balance });
        }

        // DELETE api/market/{id} — снять с продажи
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var item = await _db.Items.FindAsync(id);
            if (item == null) return NotFound();
            if (item.SellerId != userId) return Forbid();
            if (item.IsSold) return BadRequest(new { message = "Предмет вже продано" });

            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Знято з продажу" });
        }

        // GET api/market/inventory — свой инвентарь
        [HttpGet("inventory")]
        [Authorize]
        public async Task<IActionResult> GetInventory()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var inventory = await _db.InventoryItems
                .Where(ii => ii.UserId == userId)
                .Include(ii => ii.Item)
                    .ThenInclude(i => i.Game)
                .OrderByDescending(ii => ii.AcquiredAt)
                .Select(ii => new {
                    ii.Id,
                    ii.AcquiredAt,
                    Item = new
                    {
                        ii.Item.Id,
                        ii.Item.Name,
                        ii.Item.Photo,
                        ii.Item.Price,
                        ii.Item.Description,
                        Game = new { ii.Item.Game.Id, ii.Item.Game.Name }
                    }
                })
                .ToListAsync();

            return Ok(inventory);
        }

        // POST api/market/inventory/{inventoryItemId}/sell — юзер выставляет свой предмет
        [HttpPost("inventory/{inventoryItemId}/sell")]
        [Authorize]
        public async Task<IActionResult> SellFromInventory(int inventoryItemId, [FromBody] SellItemRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var invItem = await _db.InventoryItems
                .Include(ii => ii.Item)
                .FirstOrDefaultAsync(ii => ii.Id == inventoryItemId && ii.UserId == userId);

            if (invItem == null)
                return NotFound(new { message = "Предмет не знайдено в інвентарі" });

            if (request.Price <= 0)
                return BadRequest(new { message = "Ціна повинна бути більше 0" });

            // создаём новый лот на рынке
            var newItem = new Item
            {
                Name = invItem.Item.Name,
                Description = invItem.Item.Description,
                Photo = invItem.Item.Photo,
                Price = request.Price,
                GameId = invItem.Item.GameId,
                SellerId = userId
            };

            _db.Items.Add(newItem);

            // удаляем из инвентаря
            _db.InventoryItems.Remove(invItem);

            await _db.SaveChangesAsync();

            return Ok(new { newItem.Id, newItem.Name, newItem.Price });
        }

        // GET api/market/inventory/my — инвентарь для продажи
        [HttpGet("inventory/my")]
        [Authorize]
        public async Task<IActionResult> GetMyInventory()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var inventory = await _db.InventoryItems
                .Where(ii => ii.UserId == userId)
                .Include(ii => ii.Item)
                    .ThenInclude(i => i.Game)
                .OrderByDescending(ii => ii.AcquiredAt)
                .Select(ii => new {
                    ii.Id,
                    ii.AcquiredAt,
                    Item = new {
                        ii.Item.Id, ii.Item.Name, ii.Item.Photo,
                        ii.Item.Price, ii.Item.Description,
                        Game = new { ii.Item.Game.Id, ii.Item.Game.Name }
                    }
                })
                .ToListAsync();

            return Ok(inventory);
        }
    }

    public class CreateItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int GameId { get; set; }
    }

    public class SellItemRequest
    {
        public decimal Price { get; set; }
    }

}