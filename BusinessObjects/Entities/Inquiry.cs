using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Inquiry
{
    [Key]
    public Guid InquiryId { get; set; }
    [EmailAddress] public string Email { get; set; }
    public string Fullname { get; set; }
    public string Phone { get; set; }
    public string Message { get; set; }
    public DateTime CreatedDate { get; set; }
    public Account Member { get; set; }
    public Guid MemberId { get; set; }
    public Shop Shop { get; set; }
    public Guid ShopId { get; set; }
}