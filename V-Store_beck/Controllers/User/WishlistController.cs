using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/wishlist")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext _db;

        public WishlistController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/wishlist
        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var items = await _db.WishlistItems
                .Where(w => w.UserId == userId)
                .Include(w => w.Game)
                .OrderByDescending(w => w.AddedAt)
                .Select(w => new {
                    w.Id,
                    w.AddedAt,
                    Game = new
                    {
                        w.Game.Id,
                        w.Game.Name,
                        w.Game.Surname,
                        w.Game.Photo,
                        w.Game.GPA,
                        w.Game.Price
                    }
                })
                .ToListAsync();

            return Ok(items);
        }

        // POST api/wishlist/{gameId}
        [HttpPost("{gameId}")]
        public async Task<IActionResult> Add(int gameId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var gameExists = await _db.Game.AnyAsync(g => g.Id == gameId);
            if (!gameExists) return NotFound(new { message = "Гру не знайдено" });

            var already = await _db.WishlistItems.AnyAsync(w => w.UserId == userId && w.GameId == gameId);
            if (already) return BadRequest(new { message = "Вже у списку бажань" });

            _db.WishlistItems.Add(new WishlistItem { UserId = userId, GameId = gameId });
            await _db.SaveChangesAsync();

            return Ok(new { message = "Додано до списку бажань" });
        }

        // DELETE api/wishlist/{gameId}
        [HttpDelete("{gameId}")]
        public async Task<IActionResult> Remove(int gameId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var item = await _db.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.GameId == gameId);

            if (item == null) return NotFound(new { message = "Не знайдено у списку бажань" });

            _db.WishlistItems.Remove(item);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Видалено зі списку бажань" });
        }

        // GET api/wishlist/ids — список id игр в вишлисте (для кнопки)
        [HttpGet("ids")]
        public async Task<IActionResult> GetIds()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ids = await _db.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.GameId)
                .ToListAsync();
            return Ok(ids);
        }
    }
}