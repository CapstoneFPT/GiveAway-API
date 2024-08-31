using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class ConsignSaleLineItem
{
    [Key]
    public Guid ConsignSaleLineItemId { get; set; }
    public Guid ConsignSaleId { get; set; }
    public decimal? DealPrice { get; set; }
    public decimal ExpectedPrice { get; set; }
    public string? ResponseFromShop { get; set; }
    public string Note { get; set; }
    public decimal? ConfirmedPrice { get; set; }
    public ConsignSale ConsignSale { get; set; }
    public string ProductName { get; set; }
    public string Brand { get; set; }
    public string Color { get; set; }
    public SizeType Size { get; set; }
    public string Condition { get; set; }
    public GenderType Gender { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public IndividualFashionItem IndividualFashionItem { get; set; }
    public ICollection<Image> Images { get; set; } = [];
    public ConsignSaleLineItemStatus Status { get; set; }
    public bool? IsApproved { get; set; }
}