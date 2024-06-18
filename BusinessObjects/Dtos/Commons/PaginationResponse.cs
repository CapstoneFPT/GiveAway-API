namespace BusinessObjects.Dtos.Commons;

public class PaginationResponse<TDto> where TDto : class
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public string[]? Filters { get; set; }
    public string? OrderBy { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => PageNumber  < TotalPages;
    public bool HasPrevious => PageNumber > 1;
    public List<TDto> Items { get; set; } = new();
}