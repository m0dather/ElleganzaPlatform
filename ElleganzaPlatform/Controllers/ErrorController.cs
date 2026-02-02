using Microsoft.AspNetCore.Mvc;

namespace ElleganzaPlatform.Controllers;

/// <summary>
/// Error handling controller
/// Uses Store theme (Ecomus)
/// </summary>
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 404 Not Found error page
    /// </summary>
    [HttpGet("/404")]
    [HttpGet("/errors/notfound")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public new IActionResult NotFound()
    {
        Response.StatusCode = 404;
        return View();
    }

    /// <summary>
    /// Generic error handler for status codes (called by UseStatusCodePagesWithReExecute)
    /// </summary>
    [HttpGet("/error/{statusCode}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HandleStatusCode(int statusCode)
    {
        Response.StatusCode = statusCode;
        
        // Route specific status codes to appropriate views
        return statusCode switch
        {
            404 => View("NotFound"),
            403 => RedirectToAction("AccessDenied", "Account", new { area = "Identity" }),
            _ => View("Error") // Generic error view
        };
    }
}
