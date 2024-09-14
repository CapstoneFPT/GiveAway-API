namespace BusinessObjects.Dtos.Feedbacks;

public class FeedbackResponse
{
    public Guid FeedbackId { get; set; }
    public string Content { get; set; }
    public DateTime CreateDate { get; set; }
    public Guid OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
}