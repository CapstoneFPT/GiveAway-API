namespace BusinessObjects.Entities;

public class Staff : Account
{
    public Shop Shop { get; set; }
    public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}