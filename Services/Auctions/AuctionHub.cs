using BusinessObjects.Dtos.Bids;
using Microsoft.AspNetCore.SignalR;

namespace Services.Auctions;

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
        var auctionId = Context.GetHttpContext()!.Request.Query["auctionId"];
        if (!string.IsNullOrEmpty(auctionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());
        }

        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var auctionId = Context.GetHttpContext()!.Request.Query["auctionId"];
        if (!string.IsNullOrEmpty(auctionId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task PlaceBid(Guid auctionId, CreateBidRequest request)
    {
        var bidResponse = await _auctionService.PlaceBid(auctionId, request);
        if (bidResponse !=null)
        {
            await Clients.Group(auctionId.ToString()).ReceiveBidUpdate(bidResponse);
        }
        else
        {
            await Clients.Caller.ReceiveErrorMessage("Bid not created");
        }
    }
}

public interface IAuctionClient
{
    Task ReceiveBidUpdate(BidDetailResponse bid);
    Task AuctionEnded(Guid auctionId);
    Task ReceiveErrorMessage(string message);
}