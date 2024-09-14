using System.ComponentModel.DataAnnotations;
using Org.BouncyCastle.Bcpg;

namespace BusinessObjects.Entities;

public class OrderLineItem
{
    [Key]
    public Guid OrderLineItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Order Order { get; set; }
    public Guid OrderId { get; set; }
    public Refund? Refund { get; set; }
    public DateTime? RefundExpirationDate { get; set; }
    public IndividualFashionItem IndividualFashionItem { get; set; }
    public Guid? IndividualFashionItemId { get; set; }
    public DateTime? ReservedExpirationDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    
}