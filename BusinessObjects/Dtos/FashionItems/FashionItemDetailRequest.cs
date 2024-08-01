using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems
{
    public class FashionItemDetailRequest
    {
        [Required]
        
        public int SellingPrice { get; set; }
        [Required]
        public string Name { get; set; }
       
        public string? Note { get; set; }
        [Required]
        public string Description { get; set; }
        /*[Required]
        public int Quantity { get; set; }*/
        
        [Required]
        public int Condition { get; set; }
        public string? Brand { get; set; } = "No Brand";
        public required string Color { get; set; }
        public required GenderType Gender { get; set; }
        public required SizeType Size { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        [Required]
        public List<string> Images { get; set; }
    }
}
