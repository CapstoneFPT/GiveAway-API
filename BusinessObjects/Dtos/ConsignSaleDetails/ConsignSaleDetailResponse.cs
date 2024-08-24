using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSaleDetails
{
    public class ConsignSaleDetailResponse
    {
        public Guid ConsignSaleDetailId { get; set; }
        public Guid ConsignSaleId { get; set; }
        public decimal DealPrice { get; set; }
        public string Note { get; set; }
        public decimal? ConfirmedPrice { get; set; }
        public string ProductName { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public SizeType Size { get; set; }
        public GenderType Gender { get; set; }
        public string Condition { get; set; }
        public DateTime CreatedDate { get; set; }
        
        public List<string> Images { get; set; } = [];
    }

    public class ConsignSaleDetailResponse2
    {
        public Guid ConsignSaleDetailId { get; set; }
        public Guid ConsignSaleId { get; set; }
        public decimal DealPrice { get; set; }
        public decimal ConfirmedPrice { get; set; }
        public string Note { get; set; }
    }
}