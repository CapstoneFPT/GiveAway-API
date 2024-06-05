using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace BusinessObjects.Entities;

public class Request
{
   [Key]
   public Guid RequestId { get; set; } 
   public string Type { get; set; }
   public DateTime CreatedDate { get; set; }
   public int? ConsignDuration { get; set; }
   public DateTime? StartDate { get; set; }
   public DateTime? EndDate { get; set; }
   public Shop Shop { get; set; }
   public Guid ShopId { get; set; }
   public Account Member { get; set; }
   public Guid MemberId { get; set; }
   public string Status { get; set; }
}