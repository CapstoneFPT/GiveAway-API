using System.ComponentModel.DataAnnotations;
using Org.BouncyCastle.Bcpg;

namespace BusinessObjects.Entities;

public class OrderDetail
{
    [Key]
    public Guid OrderDetailId { get; set; }
    public int UnitPrice { get; set; }
    public Order Order { get; set; }
    public Guid OrderId { get; set; }
    public FashionItem? FashionItem { get; set; }
    public Guid? FashionItemId { get; set; }
    public PointPackage? PointPackage { get; set; }
    public Guid? PointPackageId { get; set; }
    public ConsignSale? ConsignSale { get; set; }
    public Guid? ConsignSaleId { get; set; }
    
}