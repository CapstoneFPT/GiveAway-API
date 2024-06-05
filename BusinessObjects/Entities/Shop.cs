using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Shop
{
   [Key]
   public Guid ShopId { get; set; } 
   public string Address { get; set; }
   public Account Staff { get; set; }
   public Guid StaffId { get; set; }

   public ICollection<Inquiry> Inquiries = new List<Inquiry>();
}