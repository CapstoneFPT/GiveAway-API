using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Auctions;

public class RejectAuctionResponse
{
    public Guid AuctionId { get; set; }
    public AuctionStatus Status { get; set; }
    public AuctionFashionItemDetailResponse? AuctionFashionItem { get; set; }
}