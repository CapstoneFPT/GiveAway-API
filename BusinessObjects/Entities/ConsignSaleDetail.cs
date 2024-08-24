using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class ConsignSaleDetail
{
    [Key]
    public Guid ConsignSaleDetailId { get; set; }
    public Guid ConsignSaleId { get; set; }
    public decimal DealPrice { get; set; }
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
}