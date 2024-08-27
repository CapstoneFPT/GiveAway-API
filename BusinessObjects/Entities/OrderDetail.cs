using System.ComponentModel.DataAnnotations;
using Org.BouncyCastle.Bcpg;

namespace BusinessObjects.Entities;

public class OrderDetail
{
    [Key]
    public Guid OrderDetailId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public Order Order { get; set; }
    public Guid OrderId { get; set; }
    public Refund? Refund { get; set; }
    public DateTime? RefundExpirationDate { get; set; }
    public IndividualFashionItem IndividualFashionItem { get; set; }
    public Guid? IndividualFashionItemId { get; set; }
    public PointPackage? PointPackage { get; set; }
    public Guid? PointPackageId { get; set; }
    public Feedback? Feedback { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    
}