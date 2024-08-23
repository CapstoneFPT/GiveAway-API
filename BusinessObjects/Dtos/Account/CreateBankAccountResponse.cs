namespace BusinessObjects.Dtos.Account;

public class CreateBankAccountResponse
{
    public Guid BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public Guid MemberId { get; set; }
}