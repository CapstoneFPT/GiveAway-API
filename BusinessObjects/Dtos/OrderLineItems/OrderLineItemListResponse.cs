using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.OrderLineItems;

public class OrderLineItemListResponse
{
        public string OrderCode { get; set; }
        public Guid OrderLineItemId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public DateTime? RefundExpirationDate { get; set; }
        public DateTime? ReservedExpirationDate { get; set; }
        public Guid? ItemId { get; set; }
        public string? ItemName { get; set; }
        public FashionItemType? ItemType { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemNote { get; set; }
        public string? Condition { get; set; }
        public string? CategoryName { get; set; }
        public string? ItemColor { get; set; }
        public SizeType? ItemSize { get; set; }
        public string? ItemBrand { get; set; }
        public GenderType? ItemGender { get; set; }
        public List<string> ItemImage { get; set; } = [];
        public FashionItemStatus? ItemStatus { get; set; }
        public string? ShopAddress { get; set; }
        public Guid? ShopId { get; set; }
        public Guid? PointPackageId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? PaymentDate { get; set; }
}