namespace BusinessObjects.Dtos.Withdraws;

public class CreateWithdrawRequest
{
    public int Amount { get; set; }

    public string Bank { get; set; } = default!;
    public string BankAccountNumber { get; set; } = default!;
    public string BankAccountName { get; set; } = default!;
}