using BlazorAdmin.Servers.Core.Auth;
using BlazorAdmin.Servers.Core.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        [ApiAuthorize]
        [HttpPost]
        public async Task<IActionResult> GetRandomNumber([FromServices] IDbContextFactory<BlazorAdminDbContext> _dbFactory)
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            return Ok(new Random(context.AuditLogs.Count()).Next(0, 9999));
        }
    }
}
