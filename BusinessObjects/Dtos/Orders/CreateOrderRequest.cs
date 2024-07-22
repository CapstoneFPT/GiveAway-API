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
        public string Address { get; set; }
        public string? RecipientName { get; set; }
        public string? Phone {  get; set; }
        [EmailAddress] public string? Email { get; set; }    
        public List<Guid?> listItemId { get; set; }
    }
}
