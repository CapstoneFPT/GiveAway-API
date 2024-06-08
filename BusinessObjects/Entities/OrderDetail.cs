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
    public FashionItem? FashionItem { get; set; }
    public Guid? ItemId { get; set; }
    public PointPackage? PointPackage { get; set; }
    public Guid? PointPackageId { get; set; }
    public Request? Request { get; set; }
    public Guid? RequestId { get; set; }
    
}