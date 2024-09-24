using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Entities;

namespace BusinessObjects.Dtos.Bids;

public class BidDetailResponse
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid MemberId { get; set; }
    public decimal Amount { get; set; }
    public string Phone { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsWinning { get; set; }
    public decimal NextAmount { get; set; }
}