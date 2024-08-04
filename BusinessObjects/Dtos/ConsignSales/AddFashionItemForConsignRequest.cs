using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.ConsignSales
{
    public class AddFashionItemForConsignRequest
    {
        [Required]
        [MaxLength(100,ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }
        public string? Note { get; set; }
        [Required]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        public decimal DealPrice { get; set; }
        
        public int Condition { get; set; }
        public SizeType Size { get; set; }
        public string Color { get; set; }
        public string? Brand { get; set; } = "No Brand";
        public GenderType Gender { get; set; }
        public required string[] Images {  get; set; } 
    }
}
