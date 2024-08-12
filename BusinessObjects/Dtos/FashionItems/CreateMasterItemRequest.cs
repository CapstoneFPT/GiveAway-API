﻿using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class CreateMasterItemRequest
{
    public required string ItemCode { get; set; }
    public required string Name { get; set; }
    public required string? Brand { get; set; }
    public required string Description { get; set; }
    public required Guid CategoryId { get; set; }
    public required GenderType Gender { get; set; }
    public required string[] Images { get; set; }
    public required Guid[] ShopId { get; set; }
}