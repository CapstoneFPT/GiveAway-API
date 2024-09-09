using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Auctions;

public class GetAccountAuctionsRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? ItemCode { get; set; }
    public string? AuctionCode { get; set; }
    public bool? IsWon { get; set; }
    public string? Title { get; set; }
    public AuctionStatus[] Statuses { get; set; } = [];
}