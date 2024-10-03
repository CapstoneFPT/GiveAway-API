using System.ComponentModel.DataAnnotations;
using WebApi2._0.Domain.Enums;

namespace WebApi2._0.Domain.Entities;

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