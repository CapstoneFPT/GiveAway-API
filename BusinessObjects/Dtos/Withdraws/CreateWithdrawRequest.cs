using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Withdraws;

public class CreateWithdrawRequest
{
    [Required]
    [Range(0,int.MaxValue,ErrorMessage = "Amount must be greater than 0")]
    public int Amount { get; set; }

    [Required]
    public string Bank { get; set; } = default!;
    [Required]
    public string BankAccountNumber { get; set; } = default!;
    [Required]
    public string BankAccountName { get; set; } = default!;
}