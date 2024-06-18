using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
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
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, type: typeof(AuctionDetailResponse))]
    public async Task<ActionResult<AuctionDetailResponse>> CreateAuction([FromBody] CreateAuctionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _auctionService.CreateAuction(request);

        return CreatedAtAction(nameof(GetAuction), new { id = result.AuctionItemId }, result);
    }
    
    [HttpPut("{id}/approve")]
    public async Task<ActionResult<AuctionDetailResponse>> ApproveAuction([FromRoute] Guid id)
    {
        var result = await _auctionService.ApproveAuction(id);
        return Ok(result);
    }
    
    [HttpPut("{id}/reject")]
    public async Task<ActionResult<AuctionDetailResponse>> RejectAuction([FromRoute] Guid id)
    {
        var result = await _auctionService.RejectAuction(id);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(PaginationResponse<AuctionListResponse>))]
    public async Task<ActionResult<PaginationResponse<AuctionListResponse>>> GetAuctions(
        [FromQuery] GetAuctionsRequest request)
    {
        var result = await _auctionService.GetAuctions(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AuctionDetailResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDetailResponse>> GetAuction([FromRoute] Guid id)
    {
        var result = await _auctionService.GetAuction(id);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDetailResponse>> DeleteAuction([FromRoute] Guid id)
    {
        var result = await _auctionService.DeleteAuction(id);
        return NoContent();
    }

    [HttpPut("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AuctionDetailResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDetailResponse>> UpdateAuction([FromRoute] Guid id,
        [FromBody] UpdateAuctionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = _auctionService.UpdateAuction(id, request);
        return Ok(result);
    }

    #endregion

    #region Bids

    [HttpGet("{id}/bids")]
    public async Task<ActionResult<PaginationResponse<BidListResponse>>> GetBids([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id}/bids")]
    public async Task<ActionResult<BidDetailResponse>> CreateBid([FromRoute] Guid id,
        [FromBody] CreateBidRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        throw new NotImplementedException();
    }

    [HttpDelete("{id}/bids/{bidId}")]
    public async Task<ActionResult> DeleteBid([FromRoute] Guid id, [FromRoute] Guid bidId)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id}/bids/{bidId}")]
    public async Task<ActionResult<BidDetailResponse>> UpdateBid([FromRoute] Guid id, [FromRoute] Guid bidId,
        [FromBody] UpdateBidRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        throw new NotImplementedException();
    }

    [HttpGet("{id}/bids/{bidId}")]
    public async Task<ActionResult<BidDetailResponse>> GetBid([FromRoute] Guid id, [FromRoute] Guid bidId)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region AuctionDeposits

    [HttpGet("{id}/deposits")]
    public async Task<ActionResult<PaginationResponse<AuctionDepositListResponse>>> GetDeposits([FromRoute] Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id}/deposits")]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, type: typeof(AuctionDepositDetailResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuctionDepositDetailResponse>> CreateDeposit([FromRoute] Guid id,
        [FromBody] CreateAuctionDepositRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _auctionService.CreateDeposit(id, request);
        return CreatedAtAction(nameof(GetDeposit), new { id = result.Id }, result);
    }

    [HttpDelete("{id}/deposits/{depositId}")]
    public async Task<ActionResult> DeleteDeposit([FromRoute] Guid id, [FromRoute] Guid depositId)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id}/deposits/{depositId}")]
    public async Task<ActionResult<AuctionDepositDetailResponse>> UpdateDeposit([FromRoute] Guid id,
        [FromRoute] Guid depositId,
        [FromBody] UpdateAuctionDepositRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        throw new NotImplementedException();
    }

    [HttpGet("{id}/deposits/{depositId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AuctionDepositDetailResponse))]
    public async Task<ActionResult<AuctionDepositDetailResponse>> GetDeposit([FromRoute] Guid id,
        [FromRoute] Guid depositId)
    {
        var result = await _auctionService.GetDeposit(id, depositId);
        return Ok(result);
    }

    #endregion
}