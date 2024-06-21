namespace BusinessObjects.Dtos.Commons;

public class Result<T>
{
    public T Data { get; set; }
    public ResultStatus ResultStatus { get; set; }
    public string[] Messages { get; set; }
}

public enum ResultStatus
{
    Success,
    NotFound,
    Duplicated,
    Error,
    Empty
}

public enum Roles
{
    Staff,
    Member
}
public enum AccountStatus
{
    Active,
    Inactive,
    NotVerify
}
public enum FashionItemStatus
{
    Available,
    Sold,
    Pending,
    AwaitingAuction,
    Bidding,
    Won,
    Rejected,
    Returned
}
public enum FashionItemType
{
    ConsignedForSale,
    ConsignedForAuction
}
public enum ConsignSaleStatus
{
    Pending, 
    AwaitDelivery,
    Received,
    Completed,
    Rejected,
    Cancelled
}
public enum OrderStatus
{
    AwaitingPayment,
    OnDelivery,
    Completed,
    Cancelled
}

public enum AuctionStatus
{
    Pending,
    Rejected,
    Approved,
    OnGoing,
    Finished
}

public enum TransactionType
{
    AuctionDeposit,
    Withdraw,
    Purchase,
    Refund
}