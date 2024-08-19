using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Deliveries
{
    public class DeliveryListResponse
    {
        public Guid AddressId { get; set; }
        public string RecipientName { set; get; }
        public string Phone { set; get; }
        public string Residence { set; get; }
        public AddressType AddressType { set; get; }
        public int GhnDistrictId { get; set; }
        public int GhnWardCode { get; set; }
        public int GhnProvinceId { get; set; }
        public string AccountName { set; get; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
