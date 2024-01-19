using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorAdmin.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public IActionResult TestJwtAuth()
        {
            return Ok(User.Identity.Name);
        }
    }
}
