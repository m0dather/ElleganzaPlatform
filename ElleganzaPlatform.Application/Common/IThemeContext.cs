namespace ElleganzaPlatform.Application.Common;

/// <summary>
/// Represents the theme type for the application
/// </summary>
public enum ThemeType
{
    /// <summary>
    /// Store-facing theme for customer browsing and shopping
    /// </summary>
    Store,

    /// <summary>
    /// Admin-facing theme for back-office management
    /// </summary>
    Admin
}

/// <summary>
/// Service for resolving the current theme context per request
/// </summary>
public interface IThemeContext
{
    /// <summary>
    /// Gets the current theme type (Store or Admin)
    /// </summary>
    ThemeType ThemeType { get; }

    /// <summary>
    /// Gets the name of the current theme (e.g., "Ecomus", "Metronic")
    /// </summary>
    string ThemeName { get; }
}
