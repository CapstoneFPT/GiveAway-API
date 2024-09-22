using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Inquiries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Inquiries;

namespace WebApi.Controllers;
[Authorize]
[Route("api/inquiries")]
[ApiController] 
public class InquiryController : ControllerBase
{
    private readonly IInquiryService _inquiryService;

    public InquiryController(IInquiryService inquiryService)
    {
        _inquiryService = inquiryService;
    }
    [HttpGet]
    public async Task<ActionResult<PaginationResponse<InquiryListResponse>>> GetAllInquiries(
        [FromQuery]InquiryListRequest inquiryRequest)
    {
        var result = await _inquiryService.GetAllInquiries(inquiryRequest);
            
        return Ok(result);
    }
}