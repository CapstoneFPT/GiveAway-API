using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSales;

public class ConfirmReceivedConsignRequest
{
   
    public decimal SellingPrice { get; set; }
    public Guid CategoryId { get; set; } 
    public string Description { get; set; }
}

  /*public class FashionItemConsignUpdate
    {
        public Guid FashionItemId { get; set; }
        public int SellingPrice { get; set; }
        public Guid CategoryId { get; set; }
    }*/