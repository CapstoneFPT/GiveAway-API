using System.ComponentModel.DataAnnotations;
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
}

