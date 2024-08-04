using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class ConsignSaleDetail
{
    [Key]
    public Guid ConsignSaleDetailId { get; set; }
    public Guid ConsignSaleId { get; set; }
    public Guid FashionItemId { get; set; }
    public decimal DealPrice { get; set; }
    public decimal ConfirmedPrice { get; set; }
    public ConsignSale ConsignSale { get; set; }
    public FashionItem FashionItem { get; set; }
    public DateTime CreatedDate { get; set; }
}