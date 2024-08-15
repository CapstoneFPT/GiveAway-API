namespace BusinessObjects.Dtos.FashionItems;

public class FindMasterItemRequest
{
    public string? MasterItemCode { get; set; }
    public string? Name { get; set; }
    public Guid? MasterItemId { get; set; }
}