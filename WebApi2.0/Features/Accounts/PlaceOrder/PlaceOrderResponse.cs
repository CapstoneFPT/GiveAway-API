using WebApi2._0.Domain.Enums;

namespace WebApi2._0.Features.Accounts.PlaceOrder;

public record PlaceOrderResponse
{
    public Guid OrderId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string OrderCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public Guid? MemberId { get; set; }
    public string? CustomerName { get; set; }
    public string? RecipientName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Address { get; set; }
    public AddressType? AddressType { get; set; }
    public string? Email { get; set; }
    public decimal ShippingFee { get; set; } = 0;
    public decimal Discount { get; set; } = 0;
    public PurchaseType PurchaseType { get; set; }
    public OrderStatus Status { get; set; }
}