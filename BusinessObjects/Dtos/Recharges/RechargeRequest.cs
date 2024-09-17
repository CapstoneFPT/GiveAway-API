using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Recharges;

public class RechargePurchaseResponse
{
    public required string PaymentUrl { get; set; }
    public Guid RechargeId { get; set; }
}

public class InitiateRechargeRequest
{
    public Guid MemberId { get; set; }
    [Range(1000, 100000000, ErrorMessage = "Amount must be between 1000 VND and 100000000 VND")]
    public decimal Amount { get; set; }
}