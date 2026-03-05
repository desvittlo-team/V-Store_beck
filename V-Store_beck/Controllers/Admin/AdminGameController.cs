using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Hosting;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/admin/games")]
    [Authorize(Roles = "Admin")]
    public class AdminGameController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminGameController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

        // POST api/admin/upload
        [HttpPost("/api/admin/upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "No file" });

            var fileName = Path.GetFileName(file.FileName);
            // простая санитизация имени
            fileName = System.Text.RegularExpressions.Regex.Replace(fileName, @"[^\w\-.]", "_");

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), "pics");
            Directory.CreateDirectory(uploadsFolder); // ключевое: гарантируем существование директории

            var path = Path.Combine(uploadsFolder, fileName);
            await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await file.CopyToAsync(stream);

            return Ok(new { fileName });
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
