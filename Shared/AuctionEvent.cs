using BusinessObjects.Dtos.Bids;

namespace Shared;

public class BidPlacedEvent
{
    public Guid AuctionId { get; set; }
    public BidDetailResponse Bid { get; set; }
}

public class AuctionEndedEvent
{
    public Guid AuctionId { get; set; }
}

public interface IEventHandler<in TEvent>
{
    Task Handle(TEvent appEvent);
}
