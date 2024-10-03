namespace WebApi2._0.Domain.ValueObjects;

public class PaginationResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public string[]? Filters { get; set; }
    public string? OrderBy { get; set; }
    public int TotalCount { get; set; }
    private int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => PageNumber < TotalPages;
    public bool HasPrevious => PageNumber > 1;
    public List<T> Items { get; set; } = [];
}