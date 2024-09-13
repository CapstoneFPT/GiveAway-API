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

        
        public string? Address { get; set; }
        
        public string? RecipientName { get; set; }
        
        [Phone]
        public string Phone {  get; set; }
        [EmailAddress] public string? Email { get; set; }
        public decimal Discount { get; set; } = 0;
        public List<Guid> ItemIds { get; set; } = [];
    }

    public class ConfirmPendingOrderRequest
    {
        public FashionItemStatus ItemStatus { get; set; }
    }
}
