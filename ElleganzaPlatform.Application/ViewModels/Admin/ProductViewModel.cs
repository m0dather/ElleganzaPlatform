using ElleganzaPlatform.Domain.Enums;

namespace ElleganzaPlatform.Application.ViewModels.Admin;

public class ProductListViewModel
{
    public IEnumerable<ProductListItemViewModel> Products { get; set; } = new List<ProductListItemViewModel>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalProducts { get; set; }
}

public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public string VendorName { get; set; } = string.Empty;
    public string? MainImage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductFormViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; }
    public int VendorId { get; set; }
    public string? MainImage { get; set; }
    public IEnumerable<VendorSelectViewModel> AvailableVendors { get; set; } = new List<VendorSelectViewModel>();
}

public class VendorSelectViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
