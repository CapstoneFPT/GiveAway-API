namespace BusinessObjects.Dtos.Account;

public class UpdateBankAccountResponse
{
     public string BankName { get; set; } = null!;
     public string BankAccountName { get; set; } = null!;
     public string BankAccountNumber { get; set; } = null!;
     public bool IsDefault { get; set; }
     public Guid BankAccountId { get; set; }
     public Guid MemberId { get; set; }
}