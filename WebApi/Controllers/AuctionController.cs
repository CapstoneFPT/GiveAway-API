﻿using BusinessObjects.Dtos.AuctionDeposits;
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

        return CreatedAtAction(nameof(GetAuction), new { id = result.AuctionId }, result);
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
    public async Task<ActionResult<PaginationResponse<BidListResponse>>> GetBids([FromRoute] Guid id,
        [FromQuery] GetBidsRequest request)
    {
        var result = await _auctionService.GetBids(id, request);
        return Ok(result);
    }

    [HttpPost("{id}/bids/place_bid")]
    public async Task<ActionResult<BidDetailResponse>> PlaceBid([FromRoute] Guid id,
        [FromBody] CreateBidRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _auctionService.PlaceBid(id, request);
        return Ok(result);
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

    [HttpPost("{auctionId}/deposits/place_deposit")]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, type: typeof(AuctionDepositDetailResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuctionDepositDetailResponse>> PlaceDeposit([FromRoute] Guid auctionId,
        [FromBody] CreateAuctionDepositRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _auctionService.PlaceDeposit(auctionId, request);
        return CreatedAtAction(nameof(GetDeposit), new { auctionId = result.AuctionId, depositId = result.Id }, result);
    }

    [HttpDelete("{auctionId}/deposits/{depositId}")]
    public async Task<ActionResult> DeleteDeposit([FromRoute] Guid auctionId, [FromRoute] Guid depositId)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{auctionId}/deposits/{depositId}")]
    public async Task<ActionResult<AuctionDepositDetailResponse>> UpdateDeposit([FromRoute] Guid auctionId,
        [FromRoute] Guid depositId,
        [FromBody] UpdateAuctionDepositRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        throw new NotImplementedException();
    }

    [HttpGet("{auctionId}/deposits/{depositId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AuctionDepositDetailResponse))]
    public async Task<ActionResult<AuctionDepositDetailResponse>> GetDeposit([FromRoute] Guid auctionId,
        [FromRoute] Guid depositId)
    {
        var result = await _auctionService.GetDeposit(auctionId, depositId);
        return Ok(result);
    }

    #endregion
}