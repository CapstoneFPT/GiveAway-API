using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.OrderDetails
{
    public class OrderDetailResponse
    {
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
        public Guid OrderId { get; set; }
        public Guid? ItemId { get; set; }
    }
}
