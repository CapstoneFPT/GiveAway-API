using System.Linq.Expressions;
using BusinessObjects.Entities;

namespace Repositories.Feedbacks;

public interface IFeedbackRepository
{
    Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetFeedbacksProjection<T>(int? PageNumber, int? PageSize, Expression<Func<Feedback,bool>> predicate, Expression<Func<Feedback,T>> selector);
    Task CreateFeedback(Feedback feedback);
}