namespace BusinessObjects.Dtos.Commons;

public class GHNApiResponse<T>
{
    public string Message { get; set; }
    public int Code { get; set; }
    public T? Data { get; set; }
}