namespace BusinessObjects.Dtos.Inquiries;

public class InquiryListRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? SearchName { get; set; }
    public Guid? MemberId { get; set; }
}