using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSales;

public class CreateItemFromConsignDetailRequest
{
    public Guid? MasterItemId { get; set; }
    
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