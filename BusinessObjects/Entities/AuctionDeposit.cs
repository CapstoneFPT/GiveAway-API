using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class AuctionDeposit
{
    [Key]
    public Guid AuctionDepositId { get; set; }
    public DateTime CreatedDate { get; set; }
    public Member Member { get; set; }
    public Guid MemberId { get; set; }
    public Auction Auction { get; set; }
    public Guid AuctionId { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public string DepositCode { get; set; }
}