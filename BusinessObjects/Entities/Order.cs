using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Order
{
    [Key]
    public Guid OrderId { get; set; }
    public int TotalPrice { get; set; }
    public string OrderCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public Account Member { get; set; }
    public Guid MemberId { get; set; }
    public Guid? BidId { get; set; }
    public Bid? Bid { get; set; }
    public OrderStatus Status { get; set; }

    public Transaction? Transaction { get; set; }
    public string? RecipientName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public ICollection<OrderDetail> OrderDetails = new List<OrderDetail>();
}