using System;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Transactions;

public class ExportTransactionsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TransactionType[] Types { get; set; } = [];
    public PaymentMethod[] PaymentMethods { get; set; } = [];
    public Guid? ShopId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? SenderName { get; set; }
    public string? ReceiverName { get; set; }
    public string? TransactionCode { get; set; }
}
