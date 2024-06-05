using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class OrderDetail
{
    [Key]
    public Guid OrderDetailId { get; set; }
    public decimal UnitPrice { get; set; }
    public Order Order { get; set; }
    public Guid OrderId { get; set; }
    public Item Item { get; set; }
    public Guid ItemId { get; set; }
}