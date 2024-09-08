using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Recharges;

public class GetRechargesRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public Guid? MemberId { get; set; }
    public RechargeStatus? RechargeStatus { get; set; }
}