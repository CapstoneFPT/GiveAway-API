using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Orders
{
    public class OrderResponse
    {
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string CustomerName { get; set; }
        public string RecipientName { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public OrderStatus Status { get; set; }
    }
}
