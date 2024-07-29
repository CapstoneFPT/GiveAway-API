using System.Text.Json.Serialization;

namespace BusinessObjects.Dtos.Auctions;

public class GetAuctionsRequest
{
    public string? SearchTerm { get; set; }
    public bool GetExpiredAuctions { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
}