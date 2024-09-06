namespace BusinessObjects.Dtos.Refunds;

public class UpdateRefundRequest
{
    public string? Description { get; set; }
    public string[] RefundImages { get; set; } = [];
}