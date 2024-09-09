﻿using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Transactions;

public class GetTransactionsResponse
{
    public Guid TransactionId { get; set; }
    public string TransactionCode { get; set; } = default!;
    public string? OrderCode { get; set; }
    public string? ConsignSaleCode { get; set; }
    public string? RechargeCode { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public TransactionType Type { get; set; }
    public Guid? MemberId { get; set; }
    public string? DepositCode { get; set; }
}