namespace BusinessObjects.Dtos.Withdraws;

public class CreateWithdrawResponse
{
    public Guid WithdrawId { get; set; }
    public int Amount { get; set; }
    public Guid MemberId { get; set; }
    public DateTime CreatedDate { get; set; }
}