using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Bid
{
    [Key]
    public Guid BidId { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public Auction Auction { get; set; }
    public Guid AuctionId { get; set; }
    public Member Member { get; set; }
    public Order Order { get; set; }
    public Guid MemberId { get; set; }
    public bool IsWinning { get; set; }
}