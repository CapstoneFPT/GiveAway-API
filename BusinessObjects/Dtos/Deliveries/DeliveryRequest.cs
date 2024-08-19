using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Deliveries
{
    public class DeliveryRequest
    {
        [Required] public string RecipientName { set; get; }
        [Required, Phone] public string Phone { set; get; }

        [Required] public AddressType AddressType { set; get; }

        [Required] public int GhnProvinceId { set; get; }
        [Required] public int GhnDistrictId { set; get; }
        [Required] public int GhnWardCode { set; get; }
        [Required] public string Residence { set; get; }
    }
}