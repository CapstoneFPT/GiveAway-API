using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Transactions;

public class GetTransactionsRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public TransactionType[] Types { get; set; } = [];
    public PaymentMethod[] PaymentMethods { get; set; } = [];
    public string? TransactionCode { get; set; }
    public string? OrderCode { get; set; }
    public string? ConsignSaleCode { get; set; }
    public string? RechargeCode { get; set; }
    public string? DepositCode { get; set; }
    public string? WithdrawCode { get; set; }
    public string? RefundCode { get; set; }
}