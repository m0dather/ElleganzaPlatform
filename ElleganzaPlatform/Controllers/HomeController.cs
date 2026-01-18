using System.Diagnostics;
using ElleganzaPlatform.Application.Services;
using ElleganzaPlatform.Models;
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
    }
}
