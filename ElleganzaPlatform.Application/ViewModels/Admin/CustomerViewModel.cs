namespace ElleganzaPlatform.Application.ViewModels.Admin;

public class CustomerListViewModel
{
    public IEnumerable<CustomerListItemViewModel> Customers { get; set; } = new List<CustomerListItemViewModel>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCustomers { get; set; }
}

public class CustomerListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public IEnumerable<CustomerOrderViewModel> RecentOrders { get; set; } = new List<CustomerOrderViewModel>();
}

public class CustomerOrderViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
