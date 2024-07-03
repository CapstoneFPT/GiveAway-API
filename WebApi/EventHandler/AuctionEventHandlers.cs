using Microsoft.AspNetCore.SignalR;
using Shared;
using WebApi.Hubs;

namespace WebApi.EventHandler;

public class BidPlacedEventHandler : IEventHandler<BidPlacedEvent>
{
    private readonly IHubContext<AuctionHub, IAuctionClient> _hubContext;

    public BidPlacedEventHandler(IHubContext<AuctionHub, IAuctionClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(BidPlacedEvent appEvent)
    {
        await _hubContext.Clients.Group(appEvent.AuctionId.ToString()).ReceiveBidUpdate(appEvent.Bid);
    }
}

public class AuctionEndedEventHandler : IEventHandler<AuctionEndedEvent>
{
    private readonly IHubContext<AuctionHub, IAuctionClient> _hubContext;

    public AuctionEndedEventHandler(IHubContext<AuctionHub, IAuctionClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(AuctionEndedEvent appEvent)
    {
        await _hubContext.Clients.Group(appEvent.AuctionId.ToString()).AuctionEnded(appEvent.AuctionId);
    }
}
