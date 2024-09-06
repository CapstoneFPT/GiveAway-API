using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.ConsignSales
{
    public class ConsignSaleDetailedResponse
    {
        public Guid ConsignSaleId { get; set; }
        public ConsignSaleType Type { get; set; }
        public string ConsignSaleCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid ShopId { get; set; }
        public string? ShopAddress { get; set; }
        public Guid? MemberId { get; set; }
        public ConsignSaleStatus Status { get; set; }
        public ConsignSaleMethod ConsignSaleMethod { get; set; }
        public string? ResponseFromShop { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal SoldPrice { get; set; }
        public decimal MemberReceivedAmount { get; set; }
        public string? Consginer { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        [EmailAddress] public string? Email { get; set; }
        /*public ICollection<ConsignSaleDetailResponse>? ConsignSaleDetails { get; set; } = new List<ConsignSaleDetailResponse>();*/
        public List<ConsignSaleDetailResponse2>? ConsignSaleDetails { get; set; }
    }
}
