using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Feedbacks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Feedbacks;

namespace WebApi.Controllers;
[Authorize]
[Route("api/feedbacks")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet]
    [ProducesResponseType<PaginationResponse<FeedbackResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetFeedbacksPagination([FromQuery] FeedbackRequest request)
    {
        var result = await _feedbackService.GetAllFeedbacks(request);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                _ => StatusCode(500,
                    new ErrorResponse("Error fetching orders", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, result.Error))
            };
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType<FeedbackResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequest request)
    {
        var result = await _feedbackService.CreateFeedback(request);

        return result.ResultStatus != ResultStatus.Success ? StatusCode((int)HttpStatusCode.InternalServerError, result) : Ok(result);
    }
}