namespace BusinessObjects.Dtos.Account;

public class BankAccountsListResponse
{
    public Guid BankAccountId { get; set; }
    public string BankName { get; set; }
    public string BankAccountNumber { get; set; }
    public string BankAccountName { get; set; }
    public string BankLogo { get; set; }
    public bool IsDefault { get; set; }
}