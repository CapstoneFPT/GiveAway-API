using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSales;

public class ConfirmReceivedConsignRequest
{
    public List<FashionItemConsignUpdate> FashionItemConsignUpdates { get; set; } =
        new List<FashionItemConsignUpdate>();
}

  public class FashionItemConsignUpdate
    {
        public Guid FashionItemId { get; set; }
        public int SellingPrice { get; set; }
        public Guid CategoryId { get; set; }
    }