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

        /*public PaymentMethod PaymentMethod { get; set; }*/
        [Required]
        public string Address { get; set; }
        [Required]
        public string RecipientName { get; set; }
        [Required]
        [Phone]
        public string Phone {  get; set; }
        [EmailAddress] public string Email { get; set; }
        public List<Guid?> ItemIds { get; set; } = [];
    }
}
