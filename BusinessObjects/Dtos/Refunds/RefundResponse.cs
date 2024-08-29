using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Transactions;

namespace BusinessObjects.Dtos.Refunds
{
    public class RefundResponse
    {
        public Guid RefundId { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid OrderLineItemId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string? ResponseFromShop { get; set; }
        public int? RefundPercentage { get; set; }
        public decimal? RefundAmount { get; set; }

        public string[] Images { get; set; }
        public RefundStatus RefundStatus { get; set; }
        public OrderLineItemDetailedResponse OrderLineItemDetailedResponse { get; set; }
        public GetTransactionsResponse? TransactionsResponse { get; set; }
    }
}