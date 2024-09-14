namespace BusinessObjects.Dtos.Feedbacks;

public class FeedbackRequest
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderCode { get; set; }
    public Guid? MemberId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}