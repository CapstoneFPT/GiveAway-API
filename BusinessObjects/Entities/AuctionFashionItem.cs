namespace BusinessObjects.Entities;

public class AuctionFashionItem : FashionItem
{
    public int Duration { get; set; }
    public decimal InitialPrice { get; set; }
    public string AuctionItemStatus { get; set; }

    public ICollection<Auction> Auctions = new List<Auction>();
}