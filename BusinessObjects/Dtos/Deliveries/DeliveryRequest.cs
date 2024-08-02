using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Deliveries
{
    public class DeliveryRequest
    {
        [Required]
        public string RecipientName { set; get; }
        [Required, Phone]
        public string Phone { set; get; }
        [Required]
        public string Residence { set; get; }
        [Required]
        public string AddressType { set; get; }
    }
}
