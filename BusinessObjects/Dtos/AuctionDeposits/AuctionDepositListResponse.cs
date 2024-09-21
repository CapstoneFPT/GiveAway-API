namespace BusinessObjects.Dtos.AuctionDeposits;

public class AuctionDepositListResponse
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public DateTime DepositDate { get; set; }
    public decimal Amount { get; set; }
    public string DepositCode { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhone { get; set; }
}