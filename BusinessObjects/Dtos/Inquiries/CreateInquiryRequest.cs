using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Inquiries;

public class CreateInquiryRequest
{
    public string Fullname { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Message { get; set; }
    public Guid ShopId { get; set; }
}