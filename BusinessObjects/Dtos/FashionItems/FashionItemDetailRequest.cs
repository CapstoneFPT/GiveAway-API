using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.FashionItems
{
    public class FashionItemDetailRequest
    {
        [Required]
        public string Type { get; set; }
        [Required]
        public int SellingPrice { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Note { get; set; }
        [Required]
        public int Quantity { get; set; }   
        public int? Value { get; set; }
        [Required]
        public string Condition { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        [Required]
        public List<string> Images { get; set; }
    }
}
