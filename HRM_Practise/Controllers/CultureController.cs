using Microsoft.AspNetCore.Mvc;

namespace HRM_Practise.Controllers
{
    public class CultureController : Controller
    {
        public IActionResult SetCulture(string culture)
        {
            // Save the selected language in a cookie or session
            HttpContext.Session.SetString("Culture", culture);

            // Redirect back to the previous page
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
