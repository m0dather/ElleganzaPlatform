namespace ElleganzaPlatform.Application.ViewModels.Admin;

public class DashboardViewModel
{
    public DashboardMetrics Metrics { get; set; } = new();
}

public class DashboardMetrics
{
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int OrdersProcessedToday { get; set; }
    public int ActiveOrders { get; set; }
}
