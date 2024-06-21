using System.ComponentModel.DataAnnotations;
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
   public Wallet Wallet { get; set; }
   public Guid WalletId { get; set; }
   public AuctionDeposit? AuctionDeposit { get; set; }
}