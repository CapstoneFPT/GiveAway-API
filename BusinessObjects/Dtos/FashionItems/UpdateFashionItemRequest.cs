using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems
{
    public class UpdateFashionItemRequest
    {
        public decimal? SellingPrice { get; set; }

        public string? Note { get; set; }
        
        public string? Condition { get; set; }
        public string? Color { get; set; }

        public SizeType? Size { get; set; }
        public string[] imageUrls { get; set; } = [];
    }
}
