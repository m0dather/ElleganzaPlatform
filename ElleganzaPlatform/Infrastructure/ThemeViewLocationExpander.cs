using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Mvc.Razor;

namespace ElleganzaPlatform.Infrastructure;

/// <summary>
/// Expands Razor view locations to include theme-specific paths
/// Enables ASP.NET Core MVC to resolve views from:
/// - /Themes/Store/Ecomus/Views/{1}/{0}.cshtml
/// - /Themes/Admin/Metronic/Views/{1}/{0}.cshtml
/// Where {1} is controller name and {0} is view name
/// </summary>
public class ThemeViewLocationExpander : IViewLocationExpander
{
    private const string ThemeKey = "theme";

    /// <summary>
    /// Populates values into the route for use in ExpandViewLocations
    /// This is called once per request to determine the theme context
    /// </summary>
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.ActionContext.HttpContext.RequestServices
            .GetService(typeof(IThemeContext)) is IThemeContext themeContext)
        {
            // Store theme type and name in the context for cache key generation
            var themeValue = $"{themeContext.ThemeType}:{themeContext.ThemeName}";
            context.Values[ThemeKey] = themeValue;
        }
    }

    /// <summary>
    /// Expands view locations to include theme-specific paths
    /// Called during view resolution to provide additional search locations
    /// </summary>
    public IEnumerable<string> ExpandViewLocations(
        ViewLocationExpanderContext context,
        IEnumerable<string> viewLocations)
    {
        if (context.Values.TryGetValue(ThemeKey, out var themeValue) && themeValue != null)
        {
            var parts = themeValue.Split(':');
            if (parts.Length == 2)
            {
                var themeType = parts[0];
                var themeName = parts[1];

                // Theme-specific view locations
                // Format: /Themes/{ThemeType}/{ThemeName}/Views/{ControllerName}/{ViewName}.cshtml
                var themeViewLocations = new[]
                {
                    // Controller-specific views
                    $"/Themes/{themeType}/{themeName}/Views/{{1}}/{{0}}.cshtml",
                    
                    // Shared views
                    $"/Themes/{themeType}/{themeName}/Views/Shared/{{0}}.cshtml"
                };

                // Return theme locations first, then fall back to default locations
                return themeViewLocations.Concat(viewLocations);
            }
        }

        // If no theme context, return default locations
        return viewLocations;
    }
}
