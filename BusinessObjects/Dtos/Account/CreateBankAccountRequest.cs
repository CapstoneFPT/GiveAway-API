using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Account;

public class CreateBankAccountRequest
{
    [Required] public string BankName { get; set; } = null!;
    [Required]
    public string BankAccountName { get; set; } = null!;

    [Required] public string BankAccountNumber { get; set; } = null!;
}