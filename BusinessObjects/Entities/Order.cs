using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Order
{
    [Key] public Guid OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public string OrderCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    public Account? Member { get; set; }
    public Guid? MemberId { get; set; }
    public Guid? BidId { get; set; }
    public Bid? Bid { get; set; }
    public OrderStatus Status { get; set; }
    public PurchaseType PurchaseType { get; set; }
    public ICollection<Transaction> Transaction { get; set; } = [];
    public string? RecipientName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public int? GhnDistrictId { get; set; }
    public int? GhnWardCode { get; set; }
    public string? Email { get; set; }
    public int? GhnProvinceId { get; set; }
    public AddressType? AddressType { get; set; }
    public decimal ShippingFee { get; set; } = 0;
    public decimal Discount { get; set; } = 0;
    public Feedback? Feedback { get; set; }

    public ICollection<OrderLineItem> OrderLineItems = new List<OrderLineItem>();
}