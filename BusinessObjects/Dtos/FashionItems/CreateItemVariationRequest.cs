using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class CreateItemVariationRequest
{
    
    public string Condition { get; set; }
    public decimal Price { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public CreateIndividualItemRequest[] IndividualItems { get; set; }
}

public class CreateIndividualItemRequest
{
    public string Note { get; set; }
    public decimal SellingPrice { get; set; }
    public string[] Images { get; set; }
}