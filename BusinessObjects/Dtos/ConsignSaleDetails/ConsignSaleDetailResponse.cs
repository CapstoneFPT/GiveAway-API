using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.ConsignSaleDetails
{
    public class ConsignSaleDetailResponse
    {
        public Guid ConsignSaleDetailId { get; set; }
        public Guid ConsignSaleId { get; set; }
        public string ConsignSaleCode { get; set; }
        public decimal DealPrice { get; set; }
        public decimal ConfirmedPrice { get; set; }
        public FashionItemDetailResponse FashionItem { get; set; }
    }

    public class ConsignSaleDetailResponse2
    {
        public Guid ConsignSaleDetailId { get; set; }
        public Guid FashionItemId { get; set; }
        public int DealPrice { get; set; }
        public int ConfirmedPrice { get; set;}
        public string ItemName { get; set; }
    }
}
