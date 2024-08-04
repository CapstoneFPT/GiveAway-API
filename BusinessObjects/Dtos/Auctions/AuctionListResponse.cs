using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;

namespace BusinessObjects.Dtos.Auctions;

public class AuctionListResponse
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DepositFee { get; set; }
    public AuctionStatus Status { get; set; }
    public string ImageUrl { get; set; }
    public Guid ShopId { get; set; }
    public Guid AuctionItemId { get; set; }
}