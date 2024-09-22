using System.Net;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.AuctionDeposits;
using Services.Auctions;

namespace WebApi.Controllers;
[Authorize]
[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly IAuctionService _auctionService;
    private readonly IAuctionDepositService _auctionDepositService;

    public AuctionController(IAuctionService auctionService, IAuctionDepositService auctionDepositService)
    {
        _auctionService = auctionService;
        _auctionDepositService = auctionDepositService;
    }

    #region Auctions
    [Authorize(Roles = "Staff")]
    [HttpPost]
    [ProducesResponseType<AuctionDetailResponse>((int)HttpStatusCode.Created)]
    public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _auctionService.CreateAuction(request);

        return CreatedAtAction(nameof(GetAuction), new { id = result.AuctionId }, result);
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/approve")]
    [ProducesResponseType<AuctionDetailResponse>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> ApproveAuction([FromRoute] Guid id)
    {
        var result = await _auctionService.ApproveAuction(id);
        return Ok(result);
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/reject")]
    [ProducesResponseType<AuctionDetailResponse>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> RejectAuction([FromRoute] Guid id)
    {
        var result = await _auctionService.RejectAuction(id);
        return Ok(result);
    }
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<PaginationResponse<AuctionListResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAllAuctionsPagination(
        [FromQuery] GetAuctionsRequest request)
    {
        var result = await _auctionService.GetAuctionList(request);
        return Ok(result);
    }

    [HttpGet("{id}/leaderboard")]
    [ProducesResponseType<AuctionLeaderboardResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetAuctionLeaderboard([FromRoute] Guid id, [FromQuery] AuctionLeaderboardRequest request)
    {
        DotNext.Result<AuctionLeaderboardResponse,ErrorCode> result = await _auctionService.GetAuctionLeaderboard(id,request);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                _ => StatusCode(500,
                    new ErrorResponse("Something went wrong!", ErrorType.AuctionError,
                        HttpStatusCode.InternalServerError, result.Error))
            };
        }

        return Ok(result.Value);
    }
    [AllowAnonymous]
    [HttpGet("{id}/auction-item")]
    [ProducesResponseType<AuctionItemDetailResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetAuctionItem([FromRoute] Guid id)
    {
        DotNext.Result<AuctionItemDetailResponse,ErrorCode> result = await _auctionService.GetAuctionItem(id);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                ErrorCode.NotFound => NotFound(new ErrorResponse("Not found", ErrorType.AuctionError, HttpStatusCode.NotFound
                    , result.Error)),
                _ => StatusCode(500,
                    new ErrorResponse("Something went wrong!", ErrorType.AuctionError,
                        HttpStatusCode.InternalServerError, result.Error)),
            };
        }
        
        return Ok(result.Value);
    }
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AuctionDetailResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuction([FromRoute] Guid id, [FromQuery] Guid? memberId)
    {
        var result = await _auctionService.GetAuction(id,memberId);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /*[HttpDelete("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAuction([FromRoute] Guid id)
    {
        var result = await _auctionService.DeleteAuction(id);
        return NoContent();
    }*/

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

        var result = await _auctionService.UpdateAuction(id, request);
        return Ok(result);
    }

    #endregion

    #region Bids

    [HttpGet("{id}/bids")]
    [ProducesResponseType<PaginationResponse<BidListResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetBids([FromRoute] Guid id,
        [FromQuery] GetBidsRequest request)
    {
        var result = await _auctionService.GetBids(id, request);
        return Ok(result);
    }
    [Authorize(Roles = "Member")]
    [HttpPost("{id}/bids/place_bid")]
    [ProducesResponseType<BidDetailResponse>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> PlaceBid([FromRoute] Guid id,
        [FromBody] CreateBidRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _auctionService.PlaceBid(id, request);
        return Ok(result);
    }
    [HttpGet("{id}/bids/latest")]
    [ProducesResponseType<BidDetailResponse>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetLatestBid([FromRoute] Guid id)
    {
        var result = await _auctionService.GetLargestBid(auctionId: id);
        return Ok(result);
    }
    #endregion

    

    #region AuctionDeposits

    [HttpGet("{auctionId}/deposits")]
    [ProducesResponseType<PaginationResponse<AuctionDepositListResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAllDepositsPagination(
        [FromRoute] Guid auctionId, [FromQuery] GetDepositsRequest request)
    {
        var result = await _auctionService.GetAuctionDeposits(auctionId, request);
        return Ok(result);
    }
    [Authorize(Roles = "Member")]
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
        return CreatedAtAction(nameof(GetDepositById), new { auctionId = result.AuctionId, depositId = result.Id }, result);
    }

    [HttpGet("{auctionId}/deposits/has-deposit")]
    public async Task<ActionResult<HasMemberPlacedDepositResult>> HasDeposit([FromRoute] Guid auctionId,
        [FromQuery] CheckDepositRequest request)
    {
        bool result = await _auctionDepositService.CheckDepositAvailable(auctionId, request.MemberId);
        return Ok(new HasMemberPlacedDepositResult()
        {
            AuctionId = auctionId,
            MemberId = request.MemberId,
            HasDeposit = result
        });
    }

    /*[HttpDelete("{auctionId}/deposits/{depositId}")]
    public async Task<ActionResult> DeleteDeposit([FromRoute] Guid auctionId, [FromRoute] Guid depositId)
    {
        throw new NotImplementedException();
    }*/

    /*[HttpPut("{auctionId}/deposits/{depositId}")]
    public async Task<ActionResult<AuctionDepositDetailResponse>> UpdateDeposit([FromRoute] Guid auctionId,
        [FromRoute] Guid depositId,
        [FromBody] UpdateAuctionDepositRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        throw new NotImplementedException();
    }*/


    [HttpGet("{auctionId}/deposits/{depositId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(AuctionDepositDetailResponse))]
    public async Task<ActionResult<AuctionDepositDetailResponse>> GetDepositById([FromRoute] Guid auctionId,
        [FromRoute] Guid depositId)
    {
        var result = await _auctionService.GetDeposit(auctionId, depositId);
        return Ok(result);
    }

    #endregion


    [HttpGet("current-time")]
    [ProducesResponseType<CurrentTimeResponse>((int)HttpStatusCode.OK)]
    public IActionResult GetCurrentTime()
    {
        return Ok(new CurrentTimeResponse() { CurrentTime = DateTime.UtcNow });
    }
}

public class CurrentTimeResponse
{
    public DateTime CurrentTime { get; set; }
}
public class CheckDepositRequest
{
    public Guid MemberId { get; set; }
}

public class HasMemberPlacedDepositResult
{
    public bool HasDeposit { get; set; }
    public Guid AuctionId { get; set; }
    public Guid MemberId { get; set; }
}