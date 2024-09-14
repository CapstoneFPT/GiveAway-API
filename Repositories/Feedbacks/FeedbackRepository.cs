using System.Linq.Expressions;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Feedbacks;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly GiveAwayDbContext _giveAwayDbContext;

    public FeedbackRepository(GiveAwayDbContext giveAwayDbContext)
    {
        _giveAwayDbContext = giveAwayDbContext;
    }

    public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetFeedbacksProjection<T>(int? PageNumber, int? PageSize, Expression<Func<Feedback, bool>> predicate, Expression<Func<Feedback, T>> selector)
    {
        var query = _giveAwayDbContext.Feedbacks.AsQueryable();
        query = query.OrderByDescending(c => c.CreatedDate);
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var count = await query.CountAsync();

        var pageNumber = PageNumber ?? -1;
        var pageSize = PageSize ?? -1;

        if (pageNumber > 0 && pageSize > 0)
        {
            query = query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }

        List<T> items;
        if (selector != null)
        {
            items = await query
                .Select(selector)
                .ToListAsync();
        }
        else
        {
            items = await query
                .Cast<T>().ToListAsync();
        }

        return (items, pageNumber, pageSize, count);
    }

    public async Task CreateFeedback(Feedback feedback)
    {
        await GenericDao<Feedback>.Instance.AddAsync(feedback);
    }
}