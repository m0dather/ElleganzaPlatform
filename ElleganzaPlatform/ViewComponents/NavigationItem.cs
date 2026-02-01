namespace ElleganzaPlatform.ViewComponents;

/// <summary>
/// Navigation item model for ViewComponents
/// </summary>
public class NavigationItem
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPostAction { get; set; } = false;
}
