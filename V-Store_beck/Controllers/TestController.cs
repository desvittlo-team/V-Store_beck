
using Microsoft.AspNetCore.Mvc;

namespace V_Store_beck.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Backend works");
        }
    }
}