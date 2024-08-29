using BusinessObjects.Dtos.FashionItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.OrderLineItems
{
    public class OrderLineItemResponse<T>
    {
        public Guid OrderLineItemId { get; set; }
        public decimal UnitPrice { get; set; }
        public Guid OrderId { get; set; }
        public DateTime? RefundExpirationDate { get; set; }
        public T FashionItemDetail { get; set;}
    }
}
