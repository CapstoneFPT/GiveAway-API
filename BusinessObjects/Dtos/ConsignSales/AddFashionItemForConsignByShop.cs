using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.ConsignSales
{
    public class AddFashionItemForConsignByShop
    {
        public string Name { get; set; }
        public string? Note { get; set; }
        public string Description { get; set; }
        public decimal ConfirmedPrice { get; set; }
        public int Condition { get; set; }
        public Guid? CategoryId { get; set; }
        public SizeType Size { get; set; }
        public string Color { get; set; }
        public string? Brand { get; set; } = "No Brand";
        public GenderType Gender { get; set; }
        public required string[] Images { get; set; }
    }
}
