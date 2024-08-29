using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.OrderLineItems
{
    public class OrderLineItemRequest
    {
        public int? PageNumber { get; set; } 
        public int? PageSize { get; set; }
        public Guid? ShopId { get; set; }
    }
}
