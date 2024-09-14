using System.Linq.Expressions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Feedbacks;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using DotNext;
using LinqKit;
using Repositories.Feedbacks;
using Repositories.Orders;

namespace Services.Feedbacks;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IOrderRepository _orderRepository;

    public FeedbackService(IFeedbackRepository feedbackRepository, IOrderRepository orderRepository)
    {
        _feedbackRepository = feedbackRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Result<PaginationResponse<FeedbackResponse>, ErrorCode>> GetAllFeedbacks(FeedbackRequest request)
    {
        Expression<Func<Feedback, bool>> predicate = feedback => true;
        Expression<Func<Feedback, FeedbackResponse>> selector = feedback => new FeedbackResponse()
        {
            FeedbackId = feedback.FeedbackId,
            CreateDate = feedback.CreatedDate,
            Content = feedback.Content,
            OrderId = feedback.OrderId,
            CustomerEmail = feedback.Order.Member!.Email,
            CustomerPhone = feedback.Order.Member!.Phone,
            CustomerName = feedback.Order.Member.Fullname
        };
        if (request.OrderId != null)
        {
            predicate = predicate.And(c => c.OrderId == request.OrderId);
        }

        if (request.OrderCode != null)
        {
            predicate = predicate.And(c => c.Order.OrderCode.Contains(request.OrderCode));
        }

        if (request.StartTime != null)
        {
            predicate = predicate.And(c => c.CreatedDate > request.StartTime);
        }

        if (request.EndTime != null)
        {
            predicate = predicate.And(c => c.CreatedDate < request.EndTime);
        }

        if (request.MemberId != null)
        {
            predicate = predicate.And(c => c.Order.MemberId == request.MemberId);
        }
        (List<FeedbackResponse> Items, int Page, int PageSize, int TotalCount) =
            await _feedbackRepository.GetFeedbacksProjection<FeedbackResponse>(request.PageNumber,
                request.PageSize, predicate, selector);
        var response = new PaginationResponse<FeedbackResponse>()
        {
            Items = Items,
            PageNumber = Page,
            PageSize = PageSize,
            TotalCount = TotalCount, SearchTerm = request.OrderCode,
        };
        return new Result<PaginationResponse<FeedbackResponse>, ErrorCode>(response);
    }

    public async Task<BusinessObjects.Dtos.Commons.Result<FeedbackResponse>> CreateFeedback(CreateFeedbackRequest request)
    {
        var order = await _orderRepository.GetOrderById(request.OrderId);
        if (order is null)
        {
            throw new OrderNotFoundException();
        }
        if (order.Status != OrderStatus.Completed)
        {
            throw new OrderNotAvailableToFeedback("This order is not completed");
        }
        var feedback = new Feedback()
        {
            CreatedDate = DateTime.UtcNow,
            Content = request.Content,
            OrderId = request.OrderId
        };
        await _feedbackRepository.CreateFeedback(feedback);
        return new BusinessObjects.Dtos.Commons.Result<FeedbackResponse>()
        {
            Data = new FeedbackResponse()
            {
                FeedbackId = feedback.FeedbackId,
                OrderId = feedback.OrderId,
                Content = feedback.Content,
                CreateDate = feedback.CreatedDate
            },
            Messages = new []{"Successfully"},
            ResultStatus = ResultStatus.Success
        };
    }
}