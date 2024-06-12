namespace BusinessObjects.Dtos.Commons;

public class PaginationResponse<TDto> where TDto : class
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public string[]? Filters { get; set; }
    public string? OrderBy { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public List<TDto> Items { get; set; } = new List<TDto>();
}