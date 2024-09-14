using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Feedbacks;
using DotNext;

namespace Services.Feedbacks;

public interface IFeedbackService
{
    Task<Result<PaginationResponse<FeedbackResponse>, ErrorCode>> GetAllFeedbacks(FeedbackRequest request);
    Task<BusinessObjects.Dtos.Commons.Result<FeedbackResponse>> CreateFeedback(CreateFeedbackRequest request);
}