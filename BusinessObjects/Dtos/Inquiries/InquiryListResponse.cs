using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Inquiries;

public class InquiryListResponse
{
    public Guid InquiryId { get; set; }
    
    public string Fullname { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Message { get; set; }
    public DateTime CreatedDate { get; set; }
    public InquiryStatus Status { get; set; }
}