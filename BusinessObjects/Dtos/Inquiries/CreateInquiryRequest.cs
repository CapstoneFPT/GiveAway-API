using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Inquiries;

public class CreateInquiryRequest
{
    [Required]
    public string Message { get; set; }

}