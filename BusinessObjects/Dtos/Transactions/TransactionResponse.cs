using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Transactions;

public class TransactionResponse
{
    public Guid TransactionId { get; set; }
    public string? TransactionCode { get; set; }
    public Guid? OrderId { get; set; }
    public string? ProductCode { get; set; }
    public Guid? ConsignSaleId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid? ShopId { get; set; }
}