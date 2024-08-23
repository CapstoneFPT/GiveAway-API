namespace BusinessObjects.Dtos.Account;

public class UpdateBankAccountRequest
{
    public string? BankName { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
    public bool? IsDefault { get; set; }
    public string? BankLogo { get; set; }
}