using AspNetCore.WebAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users
                .Select(u => new { u.Id, u.Username, u.Photo, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return await GetAll();

            var users = await _db.Users
                .Where(u => u.Username.Contains(q))
                .Select(u => new { u.Id, u.Username, u.Photo, u.Role })
                .Take(20)
                .ToListAsync();

            return Ok(users);
        }
    }
}