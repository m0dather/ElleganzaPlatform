using ElleganzaPlatform.Application.Common;
using Microsoft.AspNetCore.Http;

namespace ElleganzaPlatform.Infrastructure.Services;

/// <summary>
/// Implementation of IThemeContext that resolves theme based on request path
/// </summary>
public class ThemeContext : IThemeContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ThemeType? _cachedThemeType;
    private string? _cachedThemeName;

    public ThemeContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current theme type based on request path
    /// Admin paths: /admin, /vendor, /super-admin
    /// All other paths: Store
    /// </summary>
    public ThemeType ThemeType
    {
        get
        {
            if (_cachedThemeType.HasValue)
                return _cachedThemeType.Value;

            var path = _httpContextAccessor.HttpContext?.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Check if request is for admin-related paths
            if (path.StartsWith("/admin") || 
                path.StartsWith("/vendor") || 
                path.StartsWith("/super-admin"))
            {
                _cachedThemeType = ThemeType.Admin;
            }
            else
            {
                _cachedThemeType = ThemeType.Store;
            }

            return _cachedThemeType.Value;
        }
    }

    /// <summary>
    /// Gets the theme name based on theme type
    /// Admin → "Metronic"
    /// Store → "Ecomus"
    /// </summary>
    public string ThemeName
    {
        get
        {
            if (_cachedThemeName != null)
                return _cachedThemeName;

            _cachedThemeName = ThemeType switch
            {
                ThemeType.Admin => "Metronic",
                ThemeType.Store => "Ecomus",
                _ => "Ecomus" // Default to Store theme
            };

            return _cachedThemeName;
        }
    }
}
