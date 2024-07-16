namespace BusinessObjects.Dtos.OrderDetails;

public class OrderDetailsResponse
{
    public Guid OrderDetailId { get; set; }
    public int UnitPrice { get; set; }
    public DateTime? RefundExpirationDate { get; set; }
    public string ItemName { get; set; }
    public Guid? PointPackageId { get; set; }
}