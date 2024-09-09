using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Withdraws;

public class GetWithdrawsResponse
{
    public Guid WithdrawId { get; set; }
    public int Amount { get; set; }
    public Guid MemberId { get; set; }
    public WithdrawStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Bank { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string WithdrawCode { get; set; }
}