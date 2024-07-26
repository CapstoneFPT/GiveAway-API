using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class UpdateFashionItemStatusRequest
{
    public FashionItemStatus Status { get; set; }
}