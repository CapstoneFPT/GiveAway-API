using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Feedbacks;

public class CreateFeedbackRequest
{
    public required Guid OrderId { get; set; }
    [Required]
    public string Content { get; set; }
}