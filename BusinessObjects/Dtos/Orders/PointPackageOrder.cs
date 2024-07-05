using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Orders;

public class PointPackageOrder
{
    public int TotalPrice { get; set; }
    public string OrderCode { get; set; }
    public Guid MemberId { get; set; }
    public Guid PointPackageId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
}