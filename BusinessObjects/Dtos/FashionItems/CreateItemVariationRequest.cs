using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class CreateItemVariationRequest
{
    
    public string Condition { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public int StockCount { get; set; }
    public CreateIndividualItemRequest[] IndividualItems { get; set; }
}

public class CreateIndividualItemRequest
{
    public string Condition { get; set; }

    public string Color { get; set; }
    public SizeType Size { get; set; }
    public string Note { get; set; }
    public decimal SellingPrice { get; set; }
    public required int ItemInStock { get; set; }
}

public class NegotiateConsignSaleLineRequest
{
    public decimal DealPrice { get; set; }
    public string? ResponseFromShop { get; set; }
}

public class ConfirmConsignSaleLineReadyToSaleRequest
{

    public decimal DealPrice { get; set; }
    
    
}

public class CreateIndividualAfterNegotiationRequest
{
    public Guid MasterItemId { get; set; }
}