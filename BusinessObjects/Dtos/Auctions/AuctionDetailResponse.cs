using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;

namespace BusinessObjects.Dtos.Auctions;

public class AuctionDetailResponse
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DepositFee { get; set; }
    public string Status { get; set; }
    public Guid ShopId { get; set; }
    public Guid AuctionItemId { get; set; }
    public ShopDetailResponse Shop { get; set; }
    public AuctionItemDetailResponse AuctionItem { get; set; }
}