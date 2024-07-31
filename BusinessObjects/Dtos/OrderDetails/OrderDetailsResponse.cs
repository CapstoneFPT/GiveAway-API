using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.OrderDetails;

public class OrderDetailsResponse
{
    public Guid OrderDetailId { get; set; }
    public int UnitPrice { get; set; }
    public DateTime? RefundExpirationDate { get; set; }
    public string ItemName { get; set; }
    public FashionItemStatus ItemStatus { get; set; }
    public Guid? PointPackageId { get; set; }
    public DateTime CreatedDate { get; set; }
}