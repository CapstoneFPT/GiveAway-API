using BusinessObjects.Dtos.Auctions;
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
   
   [HttpPost]
   public async Task<IActionResult> CreateAuction(CreateAuctionRequest request)
   {
      await _auctionService.CreateAuction(request);
      return Ok();
   }
}