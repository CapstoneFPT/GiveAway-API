using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Refund
{
    [Key]
    public Guid RefundId { get; set; }
    
    public string Description { get; set; }
    public DateTime CreatedDate { get; set; } 
    public Guid OrderDetailId { get; set; }
    public OrderDetail OrderDetail { get; set; }
    
    public RefundStatus RefundStatus { get; set; }
    public Transaction Transaction { get; set; }
    public ICollection<Image> Images { get; set; } = new List<Image>();

}

