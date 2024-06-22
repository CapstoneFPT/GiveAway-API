namespace BusinessObjects.Entities;

public class Member : Account
{
    public int Balance { get; set; }
    public ICollection<ConsignSale> Requests = new List<ConsignSale>();
    public ICollection<Delivery> Deliveries = new List<Delivery>();
    public ICollection<Order> Orders = new List<Order>();
    public ICollection<Bid> Bids = new List<Bid>();
    public ICollection<AuctionDeposit> AuctionDeposits = new List<AuctionDeposit>();
}