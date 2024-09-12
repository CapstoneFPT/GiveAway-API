using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Withdraws;

public class GetWithdrawsRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public WithdrawStatus? Status { get; set; }
    public string? WithdrawCode { get; set; }
}

public class GetWithdrawByAdminRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public WithdrawStatus? Status { get; set; }
    public string? WithdrawCode { get; set; }
    public Guid? MemberId { get; set; }
}