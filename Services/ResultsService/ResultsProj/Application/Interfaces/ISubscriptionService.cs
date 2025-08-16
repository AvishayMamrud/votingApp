namespace Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task SubscribeSurveyAsync(string userId, Guid surveyId);
        Task UnsubscribeSurveyAsync(string userId, Guid surveyId);
        Task SubscribeQuestionAsync(string userId, Guid surveyId);
        Task UnsubscribeQuestionAsync(string userId, Guid surveyId);
    }
}