using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Transactions;

public class AccountTransactionsListResponse
{
    public Guid TransactionId { get; set; }
    public string TransactionCode { get; set; } = default!;
    public string? OrderCode { get; set; }
    public string? ConsignSaleCode { get; set; }
    public string? RechargeCode { get; set; }
    public string? WithdrawCode { get; set; }
    public string? RefundCode { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public TransactionType Type { get; set; }
    public Guid? SenderId { get; set; }
    public Guid? ReceiverId { get; set; }
    public string? DepositCode { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal AccountBalance { get; set; }
}