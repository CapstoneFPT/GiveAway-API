using System.Text.Json.Serialization;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Auctions;

public class GetAuctionsRequest
{
    public string? Title { get; set; }
    public bool GetExpiredAuctions { get; set; }
    public AuctionStatus[] Statuses { get; set; } = []; 
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? AuctionCode { get; set; }
}