namespace BusinessObjects.Dtos.Bids;

public class GetBidsRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}