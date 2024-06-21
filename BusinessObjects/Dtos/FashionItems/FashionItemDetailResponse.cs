using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.FashionItems
{
    public class FashionItemDetailResponse
    {
        public Guid ItemId { get; set; }
        public string Type { get; set; }
        public decimal SellingPrice { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int Quantity { get; set; }
        public decimal? Value { get; set; }
        public string Condition { get; set; }
        public int? ConsignDuration { get; set; }
        public string Status { get; set; }
        public string ShopAddress { get; set; }
        public Guid ShopId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Consigner { get; set; }
        public string CategoryName { get; set; }
    }
}
