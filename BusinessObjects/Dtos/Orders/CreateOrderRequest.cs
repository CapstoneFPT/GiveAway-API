using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Orders
{
    public class CreateOrderRequest
    {
        [Required]
        public Guid MemberId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public Guid DeliveryId { get; set; }
    }
}
