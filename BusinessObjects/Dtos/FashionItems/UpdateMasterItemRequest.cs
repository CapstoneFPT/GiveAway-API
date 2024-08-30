using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class UpdateMasterItemRequest
{
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public GenderType? Gender { get; set; }
    public int? StockCount { get; set; }
    public UpdateMasterImageRequest[]? ImageRequests { get; set; } = [];
}

public class UpdateMasterImageRequest
{
    public Guid ImageId { get; set; }
    public string Url { get; set; }
}