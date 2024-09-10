namespace Repositories.Utils;

public static class PaginationUtils
{
    public static int GetSkip(int? page, int? pageSize)
    {
        return ((page ?? 1) - 1) * (pageSize ?? 0);
    }
    
    public static int GetTake(int? pageSize)
    {
        return pageSize ?? Int32.MaxValue;
    }
}