using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Refunds;

public class ConfirmReceivedRequest
{
    public RefundStatus Status { get; set; }
    public int RefundPercentage { get; set; }
    public string ResponseFromShop { get; set; }
}