using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSaleLineItems;

public class ConsignSaleLineItemResponse
{
    public Guid ConsignSaleLineItemId { get; set; }
    public string? ResponseFromShop { get; set; }
    public ConsignSaleLineItemStatus ConsignSaleLineItemStatus { get; set; }
    public decimal DealPrice { get; set; }
    public decimal ConfirmedPrice { get; set; }
    public bool? IsApproved { get; set; }
    public Guid? IndividualItemId { get; set; }
    public FashionItemStatus? FashionItemStatus { get; set; }
}