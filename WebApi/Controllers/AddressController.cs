using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Deliveries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Deliveries;
using Services.GiaoHangNhanh;

namespace WebApi.Controllers;

[Route("api/addresses")]
[ApiController]
public class AddressController : ControllerBase
{
    private readonly IGiaoHangNhanhService _giaoHangNhanhService;
    private readonly IDeliveryService _deliveryService;

    public AddressController(IGiaoHangNhanhService giaoHangNhanhService, IDeliveryService deliveryService)
    {
        _giaoHangNhanhService = giaoHangNhanhService;
        _deliveryService = deliveryService;
    }
    [Authorize]
    [HttpGet("provinces")]
    [ProducesResponseType<GHNApiResponse<List<GHNProvinceResponse>>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetProvinces()
    {
        var result = await _giaoHangNhanhService.GetProvinces();

        if (result.IsSuccessful)
        {
            return Ok(result.Value);
        }

        return result.Error switch
        {
            ErrorCode.DeserializationError => StatusCode((int)HttpStatusCode.InternalServerError,
                new ErrorResponse("Deserialization error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                    ErrorCode.DeserializationError)),
            ErrorCode.Unauthorized => StatusCode((int)HttpStatusCode.Unauthorized,
                new ErrorResponse("Unauthorized", ErrorType.ApiError, HttpStatusCode.Unauthorized,
                    ErrorCode.Unauthorized)),
            _ => StatusCode((int)HttpStatusCode.InternalServerError,
                new ErrorResponse("Invalid request", ErrorType.InvalidRequestError, HttpStatusCode.BadRequest,
                    ErrorCode.UnknownError))
        };
    }
    [Authorize]
    [HttpGet("districts")]
    [ProducesResponseType<GHNApiResponse<List<GHNDistrictResponse>>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetDistricts([FromQuery] int provinceId)
    {
        var result = await _giaoHangNhanhService.GetDistricts(provinceId);
        
        if (result.IsSuccessful)
        {
            return Ok(result.Value);
        }
        
        return result.Error switch
        {
            ErrorCode.DeserializationError => StatusCode((int)HttpStatusCode.InternalServerError,
                new ErrorResponse("Deserialization error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                    ErrorCode.DeserializationError)),
            ErrorCode.Unauthorized => StatusCode((int)HttpStatusCode.Unauthorized,
                new ErrorResponse("Unauthorized", ErrorType.ApiError, HttpStatusCode.Unauthorized,
                    ErrorCode.Unauthorized)),
            _ => StatusCode((int)HttpStatusCode.InternalServerError,
                new ErrorResponse("Invalid request", ErrorType.InvalidRequestError, HttpStatusCode.BadRequest,
                    ErrorCode.UnknownError))
        };
    }
    [Authorize]
    [HttpGet("wards")]
    [ProducesResponseType<GHNApiResponse<List<GHNWardResponse>>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetWards([FromQuery] int districtId)
    {
        var result = await _giaoHangNhanhService.GetWards(districtId);
        
        if (result.IsSuccessful)
        {
            return Ok(result.Value);
        }
        
        return result.Error switch
        {
            ErrorCode.DeserializationError => StatusCode((int)HttpStatusCode.InternalServerError,
                new ErrorResponse("Deserialization error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                    ErrorCode.DeserializationError)),
            ErrorCode.Unauthorized => StatusCode((int)HttpStatusCode.Unauthorized,
                new ErrorResponse("Unauthorized", ErrorType.ApiError, HttpStatusCode.Unauthorized,
                    ErrorCode.Unauthorized)),
            _ => StatusCode((int)HttpStatusCode.InternalServerError,
                new ErrorResponse("Invalid request", ErrorType.InvalidRequestError, HttpStatusCode.BadRequest,
                    ErrorCode.UnknownError))
        };
    }
    [Authorize]
    [HttpPatch("{addressId}/set-default")]
    [ProducesResponseType<DeliveryListResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> SetDefaultAddress([FromRoute] Guid addressId)
    {
        var result = await _deliveryService.SetAddressAsDefault(addressId);
        
        if (result.IsSuccessful)
        {
            return Ok(result.Value);
        }
        
        return result.Error switch
        {
            ErrorCode.DeserializationError => StatusCode((int)HttpStatusCode.InternalServerError,
                new ErrorResponse("Deserialization error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                    ErrorCode.DeserializationError)),
            ErrorCode.Unauthorized => StatusCode((int)HttpStatusCode.Unauthorized,
                new ErrorResponse("Unauthorized", ErrorType.ApiError, HttpStatusCode.Unauthorized,
                    ErrorCode.Unauthorized)),
            _ => StatusCode((int)HttpStatusCode.InternalServerError,
                new ErrorResponse("Invalid request", ErrorType.InvalidRequestError, HttpStatusCode.BadRequest,
                    ErrorCode.UnknownError))
        };
    }
}