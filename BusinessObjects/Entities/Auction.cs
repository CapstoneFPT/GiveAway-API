using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class Auction
{
    [Key]
    public Guid AuctionId { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DepositFee { get; set; }
    public string AuctionCode { get; set; }
    public Shop Shop { get; set; }
    public Guid ShopId { get; set; }
    public decimal StepIncrement { get; set; }
    public IndividualAuctionFashionItem IndividualAuctionFashionItem { get; set; }
    public Guid IndividualAuctionFashionItemId { get; set; }
    public AuctionStatus Status { get; set; }

    public ICollection<AuctionDeposit> AuctionDeposits = new List<AuctionDeposit>();
    public ICollection<Bid> Bids = new List<Bid>();
    
    public DateTime CreatedDate { get; set; }
}