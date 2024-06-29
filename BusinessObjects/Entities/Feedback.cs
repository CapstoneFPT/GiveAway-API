namespace BusinessObjects.Entities;

public class Feedback
{
    public Guid FeedbackId { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; }
    public string Content { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid ShopId { get; set; }
    public Shop Shop { get; set; }
}