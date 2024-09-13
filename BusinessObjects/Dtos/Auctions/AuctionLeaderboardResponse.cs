using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Auctions;

public class AuctionLeaderboardResponse
{
    public Guid AuctionId { get; set; }
    public PaginationResponse<LeaderboardItemListResponse> Leaderboard { get; set; }
}

public class LeaderboardItemListResponse
{
    public Guid MemberId { get; set; }
    public string Phone { get; set; }
    public decimal HighestBid { get; set; }
    public bool IsWon { get; set; }
}

public class AuctionLeaderboardRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}