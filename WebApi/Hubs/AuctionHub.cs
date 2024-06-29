using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using Microsoft.AspNetCore.SignalR;
using Services.Auctions;

namespace WebApi.Hubs;

public class AuctionHub : Hub<IAuctionClient>
{
    private readonly IAuctionService _auctionService;

    public AuctionHub(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    public async Task SendBidUpdate(Guid auctionId, BidDetailResponse bid)
    {
        await Clients.Group(auctionId.ToString()).ReceiveBidUpdate(bid);
    }

    public async Task SendAuctionEndNotification(Guid auctionId)
    {
        await Clients.Group(auctionId.ToString()).AuctionEnded(auctionId);
    }

    public async Task JoinAuctionGroup(Guid auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());
    }

    public async Task LeaveAuctionGroup(Guid auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
    }
    
    public override async Task OnConnectedAsync()
    {
        var auctionId = Context.GetHttpContext().Request.Query["auctionId"];
        if (!string.IsNullOrEmpty(auctionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, auctionId);
        }

        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var auctionId = Context.GetHttpContext().Request.Query["auctionId"];
        if (!string.IsNullOrEmpty(auctionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}

public interface IAuctionClient
{
    Task ReceiveBidUpdate(BidDetailResponse bid);
    Task AuctionEnded(Guid auctionId);
}