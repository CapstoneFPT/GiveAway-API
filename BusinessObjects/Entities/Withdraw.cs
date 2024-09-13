using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Withdraw
{
    public Guid WithdrawId { get; set; }
    public string Bank { get; set; }
    public string BankAccountNumber { get; set; }
    public string BankAccountName { get; set; }
    public int Amount { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; }
    public string WithdrawCode { get; set; }
    public Transaction Transaction { get; set; }

    public WithdrawStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
}