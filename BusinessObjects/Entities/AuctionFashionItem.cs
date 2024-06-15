namespace BusinessObjects.Entities;

public class AuctionFashionItem : FashionItem
{
    public int Duration { get; set; }
    public int InitialPrice { get; set; }

    public ICollection<Auction> Auctions = new List<Auction>();
}