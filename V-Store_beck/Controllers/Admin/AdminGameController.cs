using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VStore.Data;
using VStore.Models.Catalog;

namespace VStore.Controllers.Admin;

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

    [HttpPost]
    public async Task<IActionResult> Create(Game game)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return Ok(game);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Game updated)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null) return NotFound();

        game.Title = updated.Title;
        game.Description = updated.Description;
        game.Price = updated.Price;

        await _context.SaveChangesAsync();
        return Ok(game);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null) return NotFound();

        _context.Games.Remove(game);
        await _context.SaveChangesAsync();
        return Ok();
    }
}