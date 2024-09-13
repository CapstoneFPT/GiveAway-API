using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems
{
    public class FashionItemDetailResponse
    {
        public Guid ItemId { get; set; }
        public FashionItemType Type { get; set; }
        public string ItemCode { get; set; }
        public decimal SellingPrice { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public string Description { get; set; }
        
        public string Condition { get; set; }
        public FashionItemStatus Status { get; set; }
        public string? ShopAddress { get; set; }
        public Guid ShopId { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public SizeType Size { get; set; }
        public string Color { get; set; }
        public string? Brand { get; set; }
        public bool IsConsignment { get; set; }
        public bool IsOrderedYet {get;set;}
        public bool IsItemConsigned { get; set; }
        public GenderType Gender { get; set; }
        public List<string> Images { get; set; }
    }
}