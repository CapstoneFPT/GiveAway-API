using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Transaction
{
   [Key]
   public Guid TransactionId { get; set; } 
   public int Amount { get; set; }
   public DateTime CreatedDate { get; set; }
   public TransactionType Type { get; set; }
   public Order? Order { get; set; }
   public Guid? OrderId { get; set; }
   public Refund? Refund { get; set; }
   public Guid? RefundId { get; set; }
   public Guid? ConsignSaleId { get; set; }
   public ConsignSale ? ConsignSale { get; set; }
   public Guid? MemberId { get; set; }
   public Member? Member { get; set; }
   public string? VnPayTransactionNumber { get; set; }
   public AuctionDeposit? AuctionDeposit { get; set; }
}