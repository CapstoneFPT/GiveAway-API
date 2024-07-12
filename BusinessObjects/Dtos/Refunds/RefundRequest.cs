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
        public RefundStatus[]? Status { get; set; }
        public DateTime? PreviousTime { get; set; }
    }
}
