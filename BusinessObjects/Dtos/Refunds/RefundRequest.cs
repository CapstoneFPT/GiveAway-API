using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Refunds
{
    public class RefundRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? ShopId { get; set; }
        public RefundStatus[]? Status { get; set; }
        public DateTime? PreviousTime { get; set; }
        public Guid? MemberId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? OrderCode { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
    }
}
