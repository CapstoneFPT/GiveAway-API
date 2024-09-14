namespace BusinessObjects.Entities;

public class Feedback
{
    public Guid FeedbackId { get; set; }

    public string Content { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
}