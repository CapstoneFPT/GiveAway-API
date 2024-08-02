using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.AuctionItems
{
    public class AuctionFashionItemRequest
    {
        public string? SearchTerm { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? CategoryId { get; set; }
        public FashionItemStatus[]? Status { get; set; }
        public FashionItemType[]? Type { get; set; }
        public Guid? ShopId { get; set; }
        public GenderType? GenderType { get; set; }
    }
}
