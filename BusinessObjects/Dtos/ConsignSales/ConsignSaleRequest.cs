using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.ConsignSales
{
    public class ConsignSaleRequest
    {
        public int? PageNumber { get; set; } 
        public int? PageSize { get; set; } 
        public Guid? ShopId { get; set; }
        public string? ConsignSaleCode { get; set; }
        public ConsignSaleStatus? Status { get; set; }
        public ConsignSaleType? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime?  EndDate { get; set; }
    }
}
