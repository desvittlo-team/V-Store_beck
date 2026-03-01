using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetCore.WebAPI.Data;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class CatalogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CatalogController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/games
        [HttpGet]
        public async Task<IActionResult> GetGames()
        {
            var games = await _context.Game.ToListAsync();
            return Ok(games);
        }

        // GET api/games/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(int id)
        {
            var game = await _context.Game.FindAsync(id);
            if (game is null)
                return NotFound(new { message = "Game not found" });

            return Ok(game);
        }
    }
}
