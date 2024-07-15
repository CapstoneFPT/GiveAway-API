namespace BusinessObjects.Dtos.Feedbacks;

public class CreateFeedbackRequest
{
    public Guid MemberId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid ShopId { get; set; }
}