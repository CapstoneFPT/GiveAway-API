using WebApi2._0.Domain.Enums;

namespace WebApi2._0.Features.Orders.GetOrders;

public record GetOrdersRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public Guid? ShopId { get; set; }
    public OrderStatus[] Statuses { get; set; } = [];
    public PaymentMethod[] PaymentMethods { get; set; } = [];
    public PurchaseType[] PurchaseTypes { get; set; } = [];
    public string? Phone { get; set; }
    public string? RecipientName { get; set; }
    public string? Email { get; set; }
    public string? CustomerName { get; set; }
    public string? OrderCode { get; set; }
    public bool? IsFromAuction { get; set; }
}