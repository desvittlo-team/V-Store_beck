using AspNetCore.WebAPI.Data;
using AspNetCore.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AspNetCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/cart/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var items = await _context.CartItems
                .Include(c => c.Game)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return Ok(items);
        }

        // POST api/cart
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user is null)
                return NotFound(new { message = "User not found" });

            var game = await _context.Game.FindAsync(request.GameId);
            if (game is null)
                return NotFound(new { message = "Game not found" });

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.GameId == request.GameId);

            if (existing is not null)
            {
                existing.Quantity += request.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = request.UserId,
                    GameId = request.GameId,
                    Quantity = request.Quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Added to cart" });
        }

        // DELETE api/cart/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item is null)
                return NotFound(new { message = "Cart item not found" });

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Removed from cart" });
        }

        // DELETE api/cart/clear/{userId}
        [HttpDelete("clear/{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var items = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart cleared" });
        }

        // POST api/cart/checkout/{userId}
        [HttpPost("checkout/{userId}")]
        public async Task<IActionResult> Checkout(int userId)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Game)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
                return BadRequest(new { message = "Cart is empty" });

            var order = new Order
            {
                UserId = userId,
                TotalPrice = cartItems.Sum(c => c.Game!.GPA * c.Quantity),
                Items = cartItems.Select(c => new OrderItem
                {
                    GameId = c.GameId,
                    Quantity = c.Quantity,
                    Price = c.Game!.GPA
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order created", orderId = order.Id });
        }
    }

    public class AddToCartRequest
    {
        public int UserId { get; set; }
        public int GameId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
