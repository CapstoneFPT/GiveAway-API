using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Wallet
{
    [Key]
    public Guid WalletId { get; set; }
    public int Balance { get; set; }
    public Account Member { get; set; }
    public Guid MemberId { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }

    public ICollection<Transaction> Transactions = new List<Transaction>();
}