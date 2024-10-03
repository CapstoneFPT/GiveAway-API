using System.ComponentModel.DataAnnotations;
using WebApi2._0.Domain.Enums;

namespace WebApi2._0.Features.Products.FashionItems.GetFashionItems;

public record GetFashionItemsRequest
{
    public string? ItemCode { get; set; }
    public Guid? MemberId { get; set; }
    public GenderType? Gender { get; set; }
    public string? Color { get; set; }
    public SizeType? Size { get; set; }
    public string? Condition { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public FashionItemStatus[]? Statuses { get; set; } = [];
    public FashionItemType[]? Types { get; set; } = [];
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ShopId { get; set; }
    public Guid? MasterItemId { get; set; }
    public string? MasterItemCode { get; set; }
}