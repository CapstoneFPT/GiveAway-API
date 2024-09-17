using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderLineItems;

namespace BusinessObjects.Dtos.Orders;

public class PayOrderOfflineResponse
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = "N/A";
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PurchaseType PurchaseType { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid MemberId { get; set; }
    public string? AuctionTitle { get; set; }
    public int Quantity { get; set; }
    public string ReciepientName { get; set; } = "N/A";
    public string Phone { get; set; } = "N/A";
    public string Address { get; set; } = "N/A";
    public string Email { get; set; } = "N/A";
    public OrderStatus Status { get; set; } 
    public DateTime CreatedDate { get; set; } 
}

public class VnPayPurchaseResponse
{
    public string PaymentUrl { get; set; }
}

public class PayWithPointsResponse
{
    public bool Sucess { get; set; }
    public string Message { get; set; }
    public Guid OrderId { get; set; }
}


public class PurchaseOrderRequest
{
    public Guid MemberId { get; set; }
}