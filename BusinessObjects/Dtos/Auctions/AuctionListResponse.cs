using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Auctions;

public class AuctionListResponse
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DepositFee { get; set; }
    public AuctionStatus Status { get; set; }
    public Guid ShopId { get; set; }
    public Guid AuctionItemId { get; set; }
}