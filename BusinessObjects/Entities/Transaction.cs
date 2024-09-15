using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Transaction
{
    [Key] public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public decimal SenderBalance { get; set; }
    public decimal ReceiverBalance { get; set; }
    public DateTime CreatedDate { get; set; }
    public TransactionType Type { get; set; }
    public Order? Order { get; set; }
    public Guid? OrderId { get; set; }
    public Refund? Refund { get; set; }
    public Guid? RechargeId { get; set; }
    public Recharge? Recharge { get; set; }
    public Guid? ShopId { get; set; }
    public Shop? Shop { get; set; }
    public Guid? RefundId { get; set; }
    public Guid? ConsignSaleId { get; set; }
    
    public ConsignSale? ConsignSale { get; set; }
    public Guid? WithdrawId { get; set; }
    public Withdraw? Withdraw { get; set; }
    public Guid? SenderId { get; set; }
    public Account? Sender { get; set; }
    public Guid? ReceiverId { get; set; }
    public Account? Receiver { get; set; }
    public string? VnPayTransactionNumber { get; set; }
    public Guid? AuctionDepositId { get; set; }
    public AuctionDeposit? AuctionDeposit { get; set; }
    public string TransactionCode { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}