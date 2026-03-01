using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/admin/games")]
    [Authorize(Roles = "Admin")]
    public class AdminGameController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminGameController(AppDbContext context)
        {
            _context = context;
        }

        // POST api/admin/games
        [HttpPost]
        public async Task<IActionResult> AddGame([FromBody] GameRequest request)
        {
            var game = new Game
            {
                Name = request.Name,
                Surname = request.Surname,
                Age = request.Age,
                GPA = request.GPA,
                Photo = request.Photo
            };

            _context.Game.Add(game);
            await _context.SaveChangesAsync();

            return Ok(game);
        }

        // PUT api/admin/games/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGame(int id, [FromBody] GameRequest request)
        {
            var game = await _context.Game.FindAsync(id);
            if (game is null)
                return NotFound(new { message = "Game not found" });

            game.Name = request.Name;
            game.Surname = request.Surname;
            game.Age = request.Age;
            game.GPA = request.GPA;
            game.Photo = request.Photo;

            await _context.SaveChangesAsync();

            return Ok(game);
        }

        // DELETE api/admin/games/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Game.FindAsync(id);
            if (game is null)
                return NotFound(new { message = "Game not found" });

            _context.Game.Remove(game);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game deleted" });
        }
    }

    public class GameRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public int Age { get; set; }
        public double GPA { get; set; }
        public string Photo { get; set; } = string.Empty;
    }
}
