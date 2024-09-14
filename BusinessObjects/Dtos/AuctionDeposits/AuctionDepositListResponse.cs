namespace BusinessObjects.Dtos.AuctionDeposits;

public class AuctionDepositListResponse
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public DateTime DepositDate { get; set; }
    public decimal Amount { get; set; }
    public Guid MemberId { get; set; }
}