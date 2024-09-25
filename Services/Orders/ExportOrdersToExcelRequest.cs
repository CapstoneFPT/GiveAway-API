using BusinessObjects.Dtos.Commons;

namespace Services.Orders;

public class ExportOrdersToExcelRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? OrderCode { get; set; }
    public string? RecipientName { get; set; }
    public Guid? ShopId { get; set; }
    public string? Phone { get; set; }
    public decimal? MinTotalPrice { get; set; }
    public decimal? MaxTotalPrice { get; set; }
    public PaymentMethod[] PaymentMethods { get; set; } = [];
    public PurchaseType[] PurchaseTypes { get; set; } = [];
    public OrderStatus[] Statuses { get; set; } = [];

}