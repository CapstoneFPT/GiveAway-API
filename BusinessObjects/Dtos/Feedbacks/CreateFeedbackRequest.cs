using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Feedbacks;

public class CreateFeedbackRequest
{
    public Guid MemberId { get; set; }
    [Required]
    public string Content { get; set; }
}