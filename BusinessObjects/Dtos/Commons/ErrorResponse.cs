    using System.Net;

namespace BusinessObjects.Dtos.Commons;

public class ErrorResponse
{
    public ErrorResponse(string message, ErrorType type, HttpStatusCode statusCode, ErrorCode errorCode)
    {
        Message = message;
        Type = type;
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
    
    public ErrorType Type { get; set; }
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public ErrorCode ErrorCode { get; set; }
}

public enum ErrorCode
{
    UnverifiedAccount,
    AuctionClosed,
    BankAccountNotSet,
    ScheduledTimeOverlapped,
    AuctionNotPending,
    WithdrawNotFound,
    DuplicateEmail,
    DuplicatePhoneNumber,
    UnauthorizedPayment,
    MismatchPaymentMethod,
    PointPackageNotFound,
    ExternalServiceError,
    Unauthorized,
    DeserializationError,
    UnknownError,
    NetworkError,
    ServerError,
    InvalidInput,
    NotFound,
    UnsupportedShipping,
    DuplicateBankAccount,
    NoBankAccountLeft,
    RefundStatusNotAvailable,
    MissingFeature,
    InvalidOperation,
    PaymentFailed,
    OrderAlreadyProcessed
}

public enum ErrorType
{
    ApiError,
    InvalidRequestError,
    AuctionError,
    ConsignError,
    FashionItemError,
    WithdrawError,
    PointPackageError,
    TransactionError,
    OrderError,
    PaymentError,
    AccountError,
    ShippingError,
    RefundError
}