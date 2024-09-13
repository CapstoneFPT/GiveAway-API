namespace BusinessObjects.Dtos.Bids;

public class GetBidsRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public Guid? MemberId { get; set; }
}