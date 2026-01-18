namespace ElleganzaPlatform.Application.ViewModels.Store;

public class ShopViewModel
{
    public IEnumerable<ProductCardViewModel> Products { get; set; } = new List<ProductCardViewModel>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalProducts { get; set; }
    public int PageSize { get; set; } = 12;
}
