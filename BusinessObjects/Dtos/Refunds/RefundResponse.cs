using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;

namespace BusinessObjects.Dtos.Refunds
{
    public class RefundResponse
    {
        public Guid RefundId { get; set; }

        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid OrderDetailId { get; set; }
        
        public string[] Images { get; set; }
        public RefundStatus RefundStatus { get; set; }
        public OrderDetailsResponse OrderDetailsResponse { get; set; }
    }
}
