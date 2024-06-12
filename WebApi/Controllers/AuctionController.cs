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
    public async Task<ActionResult<AuctionDetailResponse>> CreateAuction([FromBody] CreateAuctionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        throw new NotImplementedException();
    }

    [HttpGet]
    public async Task<PaginationResponse<AuctionListResponse>> GetAuctions()
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDetailResponse>> GetAuction([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AuctionDetailResponse>> UpdateAuction([FromRoute] Guid id,
        [FromBody] UpdateAuctionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        throw new NotImplementedException();
    }

    #endregion

    #region Bids

    [HttpGet("{id}/bids")]
    public async Task<ActionResult<PaginationResponse<BidListResponse>>> GetBids([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id}/bids")]
    public async Task<ActionResult<BidDetailResponse>> CreateBid([FromRoute]Guid id, [FromBody]CreateBidRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        throw new NotImplementedException();
    }

    [HttpDelete("{id}/bids/{bidId}")]
    public async Task<ActionResult> DeleteBid([FromRoute]Guid id, [FromRoute]Guid bidId)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id}/bids/{bidId}")]
    public async Task<ActionResult<BidDetailResponse>> UpdateBid([FromRoute]Guid id, [FromRoute]Guid bidId, [FromBody]UpdateBidRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        throw new NotImplementedException();
    }

    [HttpGet("{id}/bids/{bidId}")]
    public async Task<ActionResult<BidDetailResponse>> GetBid([FromRoute]Guid id, [FromRoute]Guid bidId)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region AuctionDeposits

    [HttpGet("{id}/deposits")]
    public async Task<ActionResult<PaginationResponse<AuctionDepositListResponse>>> GetDeposits([FromRoute]Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id}/deposits")]
    public async Task<ActionResult<AuctionDepositDetailResponse>> CreateDeposit([FromRoute]Guid id,
        [FromBody]CreateAuctionDepositRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        throw new NotImplementedException();
    }

    [HttpDelete("{id}/deposits/{depositId}")]
    public async Task<ActionResult> DeleteDeposit([FromRoute]Guid id, [FromRoute]Guid depositId)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id}/deposits/{depositId}")]
    public async Task<ActionResult<AuctionDepositDetailResponse>> UpdateDeposit([FromRoute]Guid id, [FromRoute] Guid depositId,
        [FromBody]UpdateAuctionDepositRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        throw new NotImplementedException();
    }

    [HttpGet("{id}/deposits/{depositId}")]
    public async Task<ActionResult<AuctionDepositDetailResponse>> GetDeposit([FromRoute]Guid id, [FromRoute]Guid depositId)
    {
        throw new NotImplementedException();
    }

    #endregion
}