using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VStore.Data;

namespace VStore.Controllers.User;

[ApiController]
[Route("api/games")]
public class CatalogController : ControllerBase
{
    private readonly AppDbContext _context;

    public CatalogController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Games.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null) return NotFound();
        return Ok(game);
    }
}