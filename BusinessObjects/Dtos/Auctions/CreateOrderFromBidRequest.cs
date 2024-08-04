using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Auctions;

public class CreateOrderFromBidRequest
{
    public decimal TotalPrice { get; set; }
    public string OrderCode { get; set; }
    public Guid BidId { get; set; }
    public Guid MemberId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public Guid AuctionFashionItemId { get; set; }
}