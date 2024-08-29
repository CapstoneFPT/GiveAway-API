using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderLineItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Shops
{
    public class ShopOrderResponse
    {
        public Guid ShopId { get; set; }
        public string? ShopAddress { get; set; }
        public List<OrderLineItemResponse<FashionItemDetailResponse>>? Items { get; set; } 
    }
}
