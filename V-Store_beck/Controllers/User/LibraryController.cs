using AspNetCore.WebAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/library")]
    [Authorize]
    public class LibraryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LibraryController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/library — получить библиотеку юзера
        [HttpGet]
        public async Task<IActionResult> GetLibrary()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var games = await _context.UserGames
                .Where(ug => ug.UserId == userId)
                .Include(ug => ug.Game)
                .Select(ug => new {
                    ug.Game.Id,
                    ug.Game.Name,
                    ug.Game.Surname,
                    ug.Game.Photo,
                    ug.Game.GPA,
                    ug.PurchasedAt
                })
                .ToListAsync();

            return Ok(games);
        }

        // POST api/library/buy/{gameId} — купить игру
        [HttpPost("buy/{gameId}")]
        public async Task<IActionResult> BuyGame(int gameId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users.FindAsync(userId);
            if (user is null) return NotFound(new { message = "User not found" });

            var game = await _context.Game.FindAsync(gameId);
            if (game is null) return NotFound(new { message = "Game not found" });

            // проверка — уже куплена?
            var already = await _context.UserGames
                .AnyAsync(ug => ug.UserId == userId && ug.GameId == gameId);
            if (already) return BadRequest(new { message = "Уже куплено" });

            // проверка баланса
            if (user.Balance < game.Price)
                return BadRequest(new { message = "Недостаточно средств" });

            user.Balance -= game.Price;
            _context.UserGames.Add(new AspNetCore.WebAPI.Models.UserGame
            {
                UserId = userId,
                GameId = gameId
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Куплено!", balance = user.Balance });
        }

        // GET api/library/balance — получить баланс
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return NotFound();
            return Ok(new { balance = user.Balance });
        }
        [HttpPost("topup")]
        public async Task<IActionResult> Topup([FromBody] TopupRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return NotFound();
            if (request.Amount <= 0) return BadRequest(new { message = "Невірна сума" });
            user.Balance += request.Amount;
            await _context.SaveChangesAsync();
            return Ok(new { balance = user.Balance });
        }

        public class TopupRequest
        {
            public decimal Amount { get; set; }
        }
    }
}