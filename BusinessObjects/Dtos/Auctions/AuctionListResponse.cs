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
    public decimal? InitialPrice { get; set; }
    public string AuctionCode { get; set; }
    public string ItemCode { get; set; }
    public decimal? SucessfulBidAmount { get; set; }
    public AuctionStatus Status { get; set; }
    public string ImageUrl { get; set; }
    public Guid ShopId { get; set; }
    public Guid AuctionItemId { get; set; }
    public bool? IsWon { get; set; }
}