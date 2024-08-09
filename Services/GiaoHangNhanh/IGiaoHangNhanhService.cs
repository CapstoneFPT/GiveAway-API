using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BusinessObjects.Dtos.Commons;
using DotNext;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GHNProvinceResponse = BusinessObjects.Dtos.Commons.GHNProvinceResponse;

namespace Services.GiaoHangNhanh;

public interface IGiaoHangNhanhService
{
    public Task<Result<GHNApiResponse<List<GHNProvinceResponse>>, ErrorCode>> GetProvinces();
    Task<Result<GHNApiResponse<List<GHNDistrictResponse>>, ErrorCode>> GetDistricts(int provinceId);
    Task<Result<GHNApiResponse<List<GHNWardResponse>>,ErrorCode>> GetWards(int districtId);
}

public class GiaoHangNhanhService : IGiaoHangNhanhService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GiaoHangNhanhService> _logger;
    private readonly IConfiguration _configuration;

    private readonly string _apiToken;

    public GiaoHangNhanhService(HttpClient httpClient, ILogger<GiaoHangNhanhService> logger,IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;

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

    public async Task<Result<GHNApiResponse<List<GHNWardResponse>>,ErrorCode>> GetWards(int districtId)
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


                    return new Result<GHNApiResponse<List<GHNWardResponse>>,ErrorCode>(content);

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
    
    
    
}