using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Deliveries
{
    public class DeliveryResponse
    {
        public Guid DeliveryId { get; set; }
        public string RecipientName { set; get; }
        public string Phone { set; get; }
        public string Address { set; get; }
        public string AddressType { set; get; }
        public string Buyername { set; get; }
    }
}
