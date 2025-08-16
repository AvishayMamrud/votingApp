using DAL.Entities;
using Data.ENUMs;

namespace Application.Interfaces
{
    public interface IResultsLogic : IResultsUpdateHandler, ISubscriptionService
    {
        Task<QuestionResult?> GetQuestionResultAsync(Guid questionId);

        Task<IReadOnlyList<QuestionResult>> GetSurveyResultsAsync(Guid surveyId);

        Task AddQuestionResultAsync(Guid qid, string qtext, QuestionType qtype, Guid surveyId, string surveyTitle);

        // Task PushVoteCountAsync(Guid questionId, Guid optionId, int newCount);

        // Task PushRangeStatsAsync(Guid questionId, double avg, double stdDev);
    }
}