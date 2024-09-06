using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BusinessObjects.Dtos.Commons;
using DotNext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repositories.Shops;
using GHNProvinceResponse = BusinessObjects.Dtos.Commons.GHNProvinceResponse;
using JsonException = System.Text.Json.JsonException;

namespace Services.GiaoHangNhanh;

public interface IGiaoHangNhanhService
{
    public Task<Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>> GetProvinces();
    Task<Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>> GetDistricts(int provinceId);
    Task<Result<GHNApiResponse<List<GHNWardResponse>>, ErrorCode>> GetWards(int districtId);
    Task<Result<GHNApiResponse<GHNShopListResponse>, ErrorCode>> GetShops();

    Task<Result<GHNApiResponse<GHNShopCreateResponse>, ErrorCode>> CreateShop(
        GHNShopCreateRequest request);

    Task<string> BuildAddress(int ghnProvinceId, int ghnDistrictId, int ghnWardCode,
        string address);

    Task<Result<GHNApiResponse<GHNShippingFee>, ErrorCode>> CalculateShippingFee(
        CalculateShippingRequest request);
}

public class GiaoHangNhanhService : IGiaoHangNhanhService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GiaoHangNhanhService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IShopRepository _shopRepository;

    private readonly string _apiToken;

    public GiaoHangNhanhService(HttpClient httpClient, ILogger<GiaoHangNhanhService> logger,
        IConfiguration configuration, IShopRepository shopRepository)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _shopRepository = shopRepository;

        _apiToken = configuration["GiaoHangNhanh:ApiToken"];
        _httpClient.DefaultRequestHeaders.Add("Token", _apiToken);
        _httpClient.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>> GetProvinces()
    {
        var url = "https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/province";

        try
        {
            var response = await _httpClient.GetAsync(url);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    var content = await response.Content.ReadFromJsonAsync<GHNApiResponse<List<GHNProvinceResponse>>>();

                    if (content == null)
                    {
                        _logger.LogWarning("Null content from GiaoHangNhanh API despite OK status");
                        return new Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>(
                            ErrorCode.DeserializationError);
                    }
                    

                    return new Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>(content);
                }
                case HttpStatusCode.Unauthorized:
                    _logger.LogWarning("Unauthorized access to GiaoHangNhanh API");
                    return new Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>(ErrorCode.Unauthorized);
                default:
                    _logger.LogWarning("Unexpected status code {StatusCode} from GiaoHangNhanh API",
                        response.StatusCode);
                    return new Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>(ErrorCode
                        .ExternalServiceError);
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Network error when accessing GiaoHangNhanh API");
            return new Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>(ErrorCode.NetworkError);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error deserializing response from GiaoHangNhanh API");
            return new Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>(ErrorCode.DeserializationError);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error when fetching provinces from GiaoHangNhanh API");
            return new Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>(ErrorCode.UnknownError);
        }
    }

    public async Task<Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>> GetDistricts(int provinceId)
    {
        var url = $"https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/district?province_id={provinceId}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var content = await response.Content.ReadFromJsonAsync<GHNApiResponse<List<GHNDistrictResponse>>>();

                    if (content == null)
                    {
                        _logger.LogWarning("Null content from GiaoHangNhanh API despite OK status");
                        return new Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>(
                            ErrorCode.DeserializationError);
                    }


                    return new Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>(content);
                case HttpStatusCode.Unauthorized:
                    return new Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>(ErrorCode.Unauthorized);
                default:
                    _logger.LogWarning("Unexpected status code {StatusCode} from GiaoHangNhanh API",
                        response.StatusCode);
                    return new Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>(ErrorCode
                        .ExternalServiceError);
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Network error when accessing GiaoHangNhanh API");
            return new Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>(ErrorCode.NetworkError);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error deserializing response from GiaoHangNhanh API");
            return new Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>(ErrorCode.DeserializationError);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error when fetching provinces from GiaoHangNhanh API");
            return new Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>(ErrorCode.UnknownError);
        }
    }

    public async Task<Result<GHNApiResponse<List<GHNWardResponse>>, ErrorCode>> GetWards(int districtId)
    {
        var url = $"https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/ward?district_id={districtId}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var content = await response.Content.ReadFromJsonAsync<GHNApiResponse<List<GHNWardResponse>>>();

                    if (content == null)
                    {
                        _logger.LogWarning("Null content from GiaoHangNhanh API despite OK status");
                        return new Result<GHNApiResponse<List<GHNWardResponse>>, ErrorCode>(ErrorCode
                            .DeserializationError);
                    }


                    return new Result<GHNApiResponse<List<GHNWardResponse>>, ErrorCode>(content);

                case HttpStatusCode.Unauthorized:
                    return new Result<GHNApiResponse<List<GHNWardResponse>>, ErrorCode>(ErrorCode.Unauthorized);
                default:
                    _logger.LogWarning("Unexpected status code {StatusCode} from GiaoHangNhanh API",
                        response.StatusCode);
                    return new Result<GHNApiResponse<List<GHNWardResponse>>, ErrorCode>(ErrorCode
                        .ExternalServiceError);
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Network error when accessing GiaoHangNhanh API");
            return new Result<GHNApiResponse<List<GHNWardResponse>>, ErrorCode>(ErrorCode.NetworkError);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error deserializing response from GiaoHangNhanh API");
            return new Result<GHNApiResponse<List<GHNWardResponse>>, ErrorCode>(ErrorCode.DeserializationError);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error when fetching provinces from GiaoHangNhanh API");
            return new Result<GHNApiResponse<List<GHNWardResponse>>, ErrorCode>(ErrorCode.UnknownError);
        }
    }

    public async Task<Result<GHNApiResponse<GHNShopListResponse>, ErrorCode>> GetShops()
    {
        var url = "https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shop/all";

        try
        {
            var response = await _httpClient.GetAsync(url);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var content = await response.Content.ReadFromJsonAsync<GHNApiResponse<GHNShopListResponse>>();
                    if (content == null)
                    {
                        _logger.LogWarning("Null content from GiaoHangNhanh API despite OK status");
                        return new Result<GHNApiResponse<GHNShopListResponse>, ErrorCode>(ErrorCode
                            .DeserializationError);
                    }

                    return new Result<GHNApiResponse<GHNShopListResponse>, ErrorCode>(content);
                case HttpStatusCode.Unauthorized:
                    return new Result<GHNApiResponse<GHNShopListResponse>, ErrorCode>(ErrorCode.Unauthorized);
                default:
                    _logger.LogWarning("Unexpected status code {StatusCode} from GiaoHangNhanh API",
                        response.StatusCode);
                    return new Result<GHNApiResponse<GHNShopListResponse>, ErrorCode>(ErrorCode
                        .ExternalServiceError);
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Network error when accessing GiaoHangNhanh API");
            return new Result<GHNApiResponse<GHNShopListResponse>, ErrorCode>(ErrorCode.NetworkError);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error deserializing response from GiaoHangNhanh API");
            return new Result<GHNApiResponse<GHNShopListResponse>, ErrorCode>(ErrorCode.DeserializationError);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error when fetching provinces from GiaoHangNhanh API");
            return new Result<GHNApiResponse<GHNShopListResponse>, ErrorCode>(ErrorCode.UnknownError);
        }
    }


    public async Task<Result<GHNApiResponse<GHNShopCreateResponse>, ErrorCode>> CreateShop(
        GHNShopCreateRequest request)
    {
        var url = "https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shop/register";

        try
        {
            var response = await _httpClient
                .SendAsync(new HttpRequestMessage(HttpMethod.Get, url + QueryString.Create(new[]
                {
                    new KeyValuePair<string, string?>("district_id", request.DistrictId.ToString()),
                    new KeyValuePair<string, string?>("ward_code", request.WardCode),
                    new KeyValuePair<string, string?>("address", request.Address),
                    new KeyValuePair<string, string?>("phone", request.Phone),
                    new KeyValuePair<string, string?>("name", await _shopRepository.GenerateShopCode()),
                })));
            _logger.LogInformation("Response message: {ResponseMessage}", response.Content.ReadAsStringAsync().Result);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var content = await response.Content.ReadFromJsonAsync<GHNApiResponse<GHNShopCreateResponse>>();
                    if (content == null)
                    {
                        _logger.LogWarning("Null content from GiaoHangNhanh API despite OK status");
                        return new Result<GHNApiResponse<GHNShopCreateResponse>, ErrorCode>(ErrorCode
                            .DeserializationError);
                    }

                    return new Result<GHNApiResponse<GHNShopCreateResponse>, ErrorCode>(content);
                case HttpStatusCode.Unauthorized:
                    return new Result<GHNApiResponse<GHNShopCreateResponse>, ErrorCode>(ErrorCode
                        .Unauthorized);
                default:
                    _logger.LogWarning("Unexpected status code {StatusCode} from GiaoHangNhanh API",
                        response.StatusCode);
                    return new Result<GHNApiResponse<GHNShopCreateResponse>, ErrorCode>(ErrorCode
                        .ExternalServiceError);
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Network error when accessing GiaoHangNhanh API");
            return new Result<GHNApiResponse<GHNShopCreateResponse>, ErrorCode>(ErrorCode
                .NetworkError);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error deserializing response from GiaoHangNhanh API");
            return new Result<GHNApiResponse<GHNShopCreateResponse>, ErrorCode>(ErrorCode
                .DeserializationError);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error when fetching provinces from GiaoHangNhanh API");
            return new Result<GHNApiResponse<GHNShopCreateResponse>, ErrorCode>(ErrorCode
                .UnknownError);
        }
    }

    public async Task<Result<GHNApiResponse<GHNShippingFee>, ErrorCode>> CalculateShippingFee(
        CalculateShippingRequest request)
    {
        var url = "https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee";

        try
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url + QueryString.Create(
                new[]
                {
                    new KeyValuePair<string, string?>("from_district_id", request.FromDistrictId.ToString()),
                    new KeyValuePair<string, string?>("to_district_id", request.ToDistrictId.ToString()),
                    new KeyValuePair<string, string?>("weight", request.Weight.ToString()),
                    new KeyValuePair<string, string?>("service_id", request.ServiceId.ToString()),
                    new KeyValuePair<string, string?>("service_type_id", request.ServiceTypeId.ToString()),
                }
            )));

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var content = await response.Content.ReadFromJsonAsync<GHNApiResponse<GHNShippingFee>>();

                    if (content == null)
                    {
                        _logger.LogWarning("Null content from GiaoHangNhanh API despite OK status");
                        return new Result<GHNApiResponse<GHNShippingFee>, ErrorCode>(ErrorCode
                            .DeserializationError);
                    }

                    return new Result<GHNApiResponse<GHNShippingFee>, ErrorCode>(content);

                case HttpStatusCode.Unauthorized:
                    return new Result<GHNApiResponse<GHNShippingFee>, ErrorCode>(ErrorCode
                        .Unauthorized);
                case HttpStatusCode.BadRequest:
                    var badRequestContent = await response.Content
                        .ReadFromJsonAsync<GHNApiResponse<GHNShop>>();

                    if (badRequestContent.Message.Contains("[GHN-ERR81]"))
                    {
                        return new Result<GHNApiResponse<GHNShippingFee>, ErrorCode>(ErrorCode.UnsupportedShipping);
                    }

                    return new Result<GHNApiResponse<GHNShippingFee>, ErrorCode>(ErrorCode.ExternalServiceError);

                default:
                    _logger.LogWarning("Unexpected status code {StatusCode} from GiaoHangNhanh API",
                        response.StatusCode);
                    return new Result<GHNApiResponse<GHNShippingFee>, ErrorCode>(ErrorCode
                        .ExternalServiceError);
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Network error when accessing GiaoHangNhanh API");
            return new Result<GHNApiResponse<GHNShippingFee>, ErrorCode>(ErrorCode
                .NetworkError);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error deserializing response from GiaoHangNhanh API");
            return new Result<GHNApiResponse<GHNShippingFee>, ErrorCode>(ErrorCode
                .DeserializationError);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error when fetching provinces from GiaoHangNhanh API");
            return new Result<GHNApiResponse<GHNShippingFee>, ErrorCode>(ErrorCode
                .UnknownError);
        }
    }

    public async Task<string> BuildAddress(int ghnProvinceId, int ghnDistrictId, int ghnWardCode,
        string address)
    {
        var ghnProvinceResponse = await GetProvinces()
            ;
        var ghnDistrictResponse = await
            GetDistricts(ghnProvinceId);
        var ghnWardResponse = await
            GetWards(ghnDistrictId);

        var province = ghnProvinceResponse
            .Value
            .Data?.Find(x => x.ProvinceId == ghnProvinceId);

        var district = ghnDistrictResponse
            .Value
            .Data?.Find(x => x.DistrictId == ghnDistrictId);

        var ward = ghnWardResponse
            .Value
            .Data?.Find(x => x.WardCode == ghnWardCode);

        return $"{address}, {ward?.WardName}, {district?.DistrictName}, {province?.ProvinceName}";
    }
}

public class GHNShippingFee
{
    public decimal Total { get; set; }
    [JsonPropertyName("service_fee")] public decimal ServiceFee { get; set; }
}

public class ShippingFeeResult
{
    public decimal ShippingFee { get; set; } = 0;
    public ShippingLocation[] ShopLocation { get; set; }
    public ShippingLocation ShippingDestination { get; set; }
}

public class ShippingLocation
{
    public string Address { get; set; }
    public int DistrictId { get; set; }
    public int WardCode { get; set; }
}

public class CalculateShippingRequest
{
    [JsonPropertyName("from_district_id")] public int FromDistrictId { get; set; }

    [JsonPropertyName("to_district_id")] public int ToDistrictId { get; set; }

    public int Weight { get; set; } = 100;

    [JsonPropertyName("service_id")] public int ServiceId { get; set; } = 53320;

    [JsonPropertyName("service_type_id")] public int ServiceTypeId { get; set; } = 2;
}