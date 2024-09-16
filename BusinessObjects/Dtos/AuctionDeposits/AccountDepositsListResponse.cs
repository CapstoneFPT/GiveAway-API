namespace BusinessObjects.Dtos.AuctionDeposits;

public class AccountDepositsListResponse
{
    public Guid AccountId { get; set; }
    public Guid DepositId { get; set; }
    public string DepositCode { get; set; }
    public string AuctionCode { get; set; }
    public bool HasRefunded { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid AuctionId { get; set; }
}