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
    Error
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
