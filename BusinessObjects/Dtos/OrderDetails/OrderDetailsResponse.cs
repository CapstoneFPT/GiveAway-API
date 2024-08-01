﻿using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.OrderDetails;

public class OrderDetailsResponse
{
    public Guid OrderDetailId { get; set; }
    public int UnitPrice { get; set; }
    public DateTime? RefundExpirationDate { get; set; }
    public string ItemName { get; set; }
    public FashionItemType ItemType { get; set; }
    public string ItemNote { get; set; }
    public int Condition { get; set; }
    public string CategoryName { get; set; }
    public string ItemColor { get; set; }
    public SizeType ItemSize { get; set; }
    public string? ItemBrand { get; set; }
    public GenderType ItemGender { get; set; }
    public List<string>? ItemImage { get; set; }
    public FashionItemStatus ItemStatus { get; set; }
    public Guid? PointPackageId { get; set; }
    public DateTime CreatedDate { get; set; }
}