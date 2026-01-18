namespace ElleganzaPlatform.Application.ViewModels.Store;

public class ProductDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? MainImage { get; set; }
    public VendorInfoViewModel Vendor { get; set; } = null!;
    public bool IsInStock => StockQuantity > 0;
    public decimal? DiscountPercentage => CompareAtPrice.HasValue && CompareAtPrice > Price 
        ? Math.Round(((CompareAtPrice.Value - Price) / CompareAtPrice.Value) * 100, 0) 
        : null;
}

public class VendorInfoViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
}
