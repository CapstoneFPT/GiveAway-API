﻿using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSaleLineItems
{
    public class ConsignSaleLineItemsListResponse
    {
        public Guid ConsignSaleLineItemId { get; set; }
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

    public class ConsignSaleLineItemDetailedResponse
    {
        public Guid ConsignSaleLineItemId { get; set; }
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
        public Guid ConsignSaleLineItemId { get; set; }
        public Guid ConsignSaleId { get; set; }
        public decimal DealPrice { get; set; }
        public decimal ConfirmedPrice { get; set; }
        public string Note { get; set; }
    }
}