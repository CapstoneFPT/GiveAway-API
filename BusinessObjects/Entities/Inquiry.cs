using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Inquiry
{
    [Key]
    public Guid InquiryId { get; set; }
    
    public string Message { get; set; }
    public DateTime CreatedDate { get; set; }
    public Account Member { get; set; }
    public Guid MemberId { get; set; }
    public InquiryStatus Status { get; set; }
}