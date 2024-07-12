namespace BusinessObjects.Dtos.Inquiries;

public class CreateInquiryResponse
{
    public Guid InquiryId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Message { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Fullname { get; set; }
    public Guid ShopId { get; set; }
    public Guid MemberId { get; set; }
}