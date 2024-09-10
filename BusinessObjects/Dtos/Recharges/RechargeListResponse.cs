using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Recharges;

public class RechargeListResponse
{
    public Guid RechargeId { get; set; }
    public Guid MemberId { get; set; }
    public decimal Amount { get; set; }
    public RechargeStatus Status { get; set; }
    public string RechargeCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}

