using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Transaction
{
   [Key]
   public Guid TransactionId { get; set; } 
   public decimal Amount { get; set; }
   public DateTime CreatedDate { get; set; }
   public string Type { get; set; }
   public Order? Order { get; set; }
   public Guid? OrderId { get; set; }
   public Wallet Wallet { get; set; }
   public Guid WalletId { get; set; }
   public AuctionDeposit? AuctionDeposit { get; set; }
}