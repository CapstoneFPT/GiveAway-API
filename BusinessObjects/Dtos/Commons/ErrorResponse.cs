using System.Net;

namespace BusinessObjects.Dtos.Commons;

public class ErrorResponse
{
    public ErrorType Type { get; set; }
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public ErrorCode ErrorCode { get; set; }
}

public enum ErrorCode
{
    
}

public enum ErrorType
{
    ApiError,
    InvalidRequestError
}