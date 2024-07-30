using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Orders
{
    public class CartRequest
    {
        public PaymentMethod PaymentMethod { get; set; }
        public string Address { get; set; }
        public string? RecipientName { get; set; }
        [Phone]
        public string? Phone { get; set; }
        public List<Guid?> listItemId { get; set; }
    }
}
