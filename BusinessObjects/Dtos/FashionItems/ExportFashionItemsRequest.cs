using System;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.FashionItems;

public class ExportFashionItemsRequest
{
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public string? ItemCode { get; set; }
  public Guid? ShopId { get; set; }
  public FashionItemStatus[]? Status { get; set; }
  public FashionItemType[]? Type { get; set; }
  public decimal? MinPrice { get; set; }
  public decimal? MaxPrice { get; set; }
}
