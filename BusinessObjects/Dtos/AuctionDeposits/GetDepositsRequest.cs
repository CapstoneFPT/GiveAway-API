namespace BusinessObjects.Dtos.AuctionDeposits;

public class GetDepositsRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? DepositCode { get; set; }
    public string? AuctionCode  { get; set; }
    public bool? IsRefunded { get; set; }
}