using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Timeslots;
using Microsoft.AspNetCore.Mvc;
using Services.Timeslots;

namespace WebApi.Controllers;

[ApiController]
[Route("api/timeslots")]
public class TimeslotController : ControllerBase
{
    private readonly ITimeslotService _timeslotService;

    public TimeslotController(ITimeslotService timeslotService)
    {
        _timeslotService = timeslotService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<TimeslotListResponse>>> GetTimeslots(
        [FromQuery] GetTimeslotsRequest request)
    {
        var result = await _timeslotService.GetTimeslotList(request);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateTimeslotResponse>> CreateTimeslot(CreateTimeslotRequest request)
    {
        var result = await _timeslotService.CreateTimeslot(request);

        return Ok(result);
    }
}