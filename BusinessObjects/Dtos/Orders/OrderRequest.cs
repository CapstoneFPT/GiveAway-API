using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Orders
{
    public class OrderRequest
    {
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public Guid? ShopId { get; set; }
        public OrderStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PurchaseType? PurchaseType { get; set; }
        public string? Phone { get; set; }
        public string? RecipientName { get; set; }
        public string? Email { get; set; }
        public string? CustomerName { get; set; }
        public string? OrderCode { get; set; }
        public bool? IsFromAuction { get; set; }
    }
}
