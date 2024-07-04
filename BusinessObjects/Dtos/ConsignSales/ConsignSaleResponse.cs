using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.ConsignSales
{
    public class ConsignSaleResponse
    {
        public Guid ConsignSaleId { get; set; }
        public ConsignSaleType Type { get; set; }
        public string ConsignSaleCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ConsignDuration { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid ShopId { get; set; }
        public Guid MemberId { get; set; }
        public ConsignSaleStatus Status { get; set; }
        public int TotalPrice { get; set; }
        public int SoldPrice { get; set; }
        public int MemberReceivedAmount { get; set; }
        public ICollection<ConsignSaleDetailResponse>? ConsignSaleDetails { get; set; } = new List<ConsignSaleDetailResponse>();
    }
}
