using System.Diagnostics;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStoreService _storeService;

        public HomeController(
            ILogger<HomeController> logger,
            IStoreService storeService)
        {
            _logger = logger;
            _storeService = storeService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _storeService.GetHomePageDataAsync();
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Sets the user's preferred language/culture
        /// </summary>
        /// <param name="culture">Culture code (e.g., "en", "ar")</param>
        /// <param name="returnUrl">URL to redirect back to after setting culture</param>
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            // Validate the culture
            var supportedCultures = new[] { "en", "ar" };
            if (!supportedCultures.Contains(culture))
            {
                culture = "en"; // Default to English if invalid
            }

            // Set the culture cookie
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions 
                { 
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Path = "/",
                    SameSite = SameSiteMode.Lax
                }
            );

            // Redirect back to the same page
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }
    }
}
