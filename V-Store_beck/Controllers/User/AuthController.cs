using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VStore.Data;
using VStore.Services.Core;

namespace VStore.Controllers.User;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AppDbContext _context;

    public AuthController(AuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string email, string password)
    {
        if (await _context.Users.AnyAsync(x => x.Email == email))
            return BadRequest("Email already exists");

        var user = await _authService.Register(username, email, password);
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Unauthorized();

        if (user.IsBlocked)
            return Forbid();

        var token = _authService.GenerateJwt(user);
        return Ok(new { token });
    }
}