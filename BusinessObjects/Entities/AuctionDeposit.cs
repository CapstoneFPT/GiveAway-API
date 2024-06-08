using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class AuctionDeposit
{
    [Key]
    public Guid AuctionDepositId { get; set; }
    public DateTime CreatedDate { get; set; }
    public Account Member { get; set; }
    public Guid MemberId { get; set; }
    public Auction Auction { get; set; }
    public Guid AuctionId { get; set; }
    public Transaction Transaction { get; set; }
    public Guid TransactionId { get; set; }
}