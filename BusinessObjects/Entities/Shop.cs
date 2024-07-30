﻿using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Shop
{
   [Key]
   public Guid ShopId { get; set; } 
   public string Address { get; set; }
   public Staff Staff { get; set; }
   public Guid StaffId { get; set; }
   public string Phone { get; set; }
   public DateTime CreatedDate { get; set; }
}