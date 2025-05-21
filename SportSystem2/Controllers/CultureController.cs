using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace SportSystem2.Controllers
{
    public class CultureController : Controller
    {
        [HttpGet]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }

            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "/";

            return LocalRedirect(returnUrl);
        }
    }
}
