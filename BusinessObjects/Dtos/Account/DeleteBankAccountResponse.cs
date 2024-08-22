namespace BusinessObjects.Dtos.Account;

public class DeleteBankAccountResponse
{
    public Guid BankAccountId { get; set; }
    public string BankName { get; set; }
    public string BankAccountName { get; set; }
    public string BankAccountNumber { get; set; }
    public Guid MemberId { get; set; }
    public bool IsDefault { get; set; }
}