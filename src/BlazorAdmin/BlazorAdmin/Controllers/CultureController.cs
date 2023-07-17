using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorAdmin.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class CultureController : ControllerBase
	{
		public IActionResult Set(string culture, string redirectUri)
		{
			if (culture != null)
			{
				HttpContext.Response.Cookies.Append(
					CookieRequestCultureProvider.DefaultCookieName,
					CookieRequestCultureProvider.MakeCookieValue(
						new RequestCulture(culture, culture)));
			}

			return LocalRedirect(redirectUri);
		}
	}
}
