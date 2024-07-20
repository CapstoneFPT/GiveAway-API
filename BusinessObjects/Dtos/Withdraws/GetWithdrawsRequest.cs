using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Withdraws;

public class GetWithdrawsRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public WithdrawStatus Status { get; set; }
}