using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class ConsignSaleDetail
{
    [Key]
    public Guid ConsignSaleDetailId { get; set; }
    public Guid ConsignSaleId { get; set; }
    public Guid FashionItemId { get; set; }
    public int DealPrice { get; set; }
    public int ConfirmedPrice { get; set; }
    public ConsignSale ConsignSale { get; set; }
    public FashionItem FashionItem { get; set; }
    public DateTime CreatedDate { get; set; }
}