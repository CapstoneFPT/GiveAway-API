using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Auctions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly IAuctionService _auctionService;

    public AuctionController(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    #region Auctions

    [HttpPost]
    public async Task<ActionResult<AuctionDetailResponse>> CreateAuction(CreateAuctionRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    public async Task<PaginationResponse<AuctionListResponse>> GetAuctions()
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDetailResponse>> GetAuction(Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AuctionDetailResponse>> UpdateAuction(Guid id, UpdateAuctionRequest request)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Bids

    [HttpGet("{id}/bids")]
    public async Task<ActionResult<PaginationResponse<BidListResponse>>> GetBids(Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id}/bids")]
    public async Task<ActionResult<BidDetailResponse>> CreateBid(Guid id, CreateBidRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id}/bids/{bidId}")]
    public async Task<ActionResult> DeleteBid(Guid id, Guid bidId)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id}/bids/{bidId}")]
    public async Task<ActionResult<BidDetailResponse>> UpdateBid(Guid id, Guid bidId, UpdateBidRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id}/bids/{bidId}")]
    public async Task<ActionResult<BidDetailResponse>> GetBid(Guid id, Guid bidId)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region AuctionDeposits

    [HttpGet("{id}/deposits")]
    public async Task<ActionResult<PaginationResponse<AuctionDepositListResponse>>> GetDeposits(Guid id)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("{id}/deposits")]
    public async Task<ActionResult<AuctionDepositDetailResponse>> CreateDeposit(Guid id, CreateAuctionDepositRequest request)
    {
        throw new NotImplementedException();
    }
    
    [HttpDelete("{id}/deposits/{depositId}")]
    public async Task<ActionResult> DeleteDeposit(Guid id, Guid depositId)
    {
        throw new NotImplementedException();
    }
    
    [HttpPut("{id}/deposits/{depositId}")]
    public async Task<ActionResult<AuctionDepositDetailResponse>> UpdateDeposit(Guid id, Guid depositId, UpdateAuctionDepositRequest request)
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("{id}/deposits/{depositId}")]
    public async Task<ActionResult<AuctionDepositDetailResponse>> GetDeposit(Guid id, Guid depositId)
    {
        throw new NotImplementedException();
    }

    #endregion
}