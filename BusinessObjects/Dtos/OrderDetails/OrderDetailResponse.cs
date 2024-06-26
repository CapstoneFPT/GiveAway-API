using BusinessObjects.Dtos.FashionItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.OrderDetails
{
    public class OrderDetailResponse<T>
    {

        public int UnitPrice { get; set; }
        public Guid OrderId { get; set; }
        public T? FashionItemDetail { get; set;}
    }
}
