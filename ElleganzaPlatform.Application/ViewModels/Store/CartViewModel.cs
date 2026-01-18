namespace ElleganzaPlatform.Application.ViewModels.Store;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems => Items.Sum(i => i.Quantity);
}

public class CartItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public string? ImageUrl { get; set; }
    public int VendorId { get; set; }
    public int StockQuantity { get; set; }
}

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
