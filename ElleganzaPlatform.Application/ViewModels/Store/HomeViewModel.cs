namespace ElleganzaPlatform.Application.ViewModels.Store;

public class HomeViewModel
{
    public IEnumerable<ProductCardViewModel> FeaturedProducts { get; set; } = new List<ProductCardViewModel>();
    public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    public IEnumerable<BrandViewModel> Brands { get; set; } = new List<BrandViewModel>();
}

public class ProductCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MainImage { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? VendorName { get; set; }
    public bool IsInStock { get; set; }
}

public class CategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class BrandViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public int ProductCount { get; set; }
}
