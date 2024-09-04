using System.ComponentModel.DataAnnotations;
using Point = NetTopologySuite.Geometries.Point;

namespace BusinessObjects.Entities;

public class Shop
{
   [Key]
   public Guid ShopId { get; set; } 
   public string Address { get; set; }
   public Staff Staff { get; set; }
   public Guid StaffId { get; set; }
   public int? GhnShopId { get; set; }
   public int? GhnDistrictId { get; set; }
   public string? GhnWardCode { get; set; }
   public string Phone { get; set; }
   public string ShopCode { get; set; }
   public Point Location { get; set; }
   public DateTime CreatedDate { get; set; }

   public ICollection<MasterFashionItem> MasterFashionItems { get; set; } = [];
   public ICollection<Transaction> Transactions { get; set; } = [];
}