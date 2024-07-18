namespace BusinessObjects.Entities;

public class Withdraw
{
    public Guid WithdrawId { get; set; }
    public int Amount { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; }
    public DateTime CreatedDate { get; set; }
}