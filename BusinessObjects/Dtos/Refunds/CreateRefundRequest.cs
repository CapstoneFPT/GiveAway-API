using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Refunds
{
    public class CreateRefundRequest{
        [Required]
        public Guid OrderLineItemId { get; set; }
        public required string Description { get; set; }
        public required string[] Images { get; set; }
        // public int RefundPercentage { get; set; }
    }
    public class CreateRefundByShopRequest{
        [Required]
        public Guid OrderLineItemId { get; set; }
        public required string Description { get; set; }
        public required string[] Images { get; set; }
        public int RefundPercentage { get; set; }
    }
}