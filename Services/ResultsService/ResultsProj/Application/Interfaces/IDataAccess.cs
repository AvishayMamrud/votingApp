using DAL.Entities;
using Data.ENUMs;

namespace Application.Interfaces
{
    /// <summary>
    /// Abstraction over database access for results-related queries and updates.
    /// </summary>
    public interface IDataAccess
    {
        // Queries
        Task<QuestionResult?> GetQuestionResultAsync(Guid questionId);
        Task<IReadOnlyList<QuestionResult>> GetSurveyResultsAsync(Guid surveyId);

        // Commands
        Task UpdateVoteCountAsync(Guid questionId, Guid optionId, long newCount);
        Task UpdateRangeStatsAsync(Guid questionId, double avg, double stdDev);
        Task AddSingleChoiceResultAsync(Guid questionResultId, Guid optionId, string optionText);
        Task AddRangeQuestionResultAsync(Guid questionResultId, int minValue, int maxValue);
        Task AddQuestionResultAsync(Guid questionId, string text, QuestionType type, Guid surveyId, string surveyTitle, long VoteCount = 0, DateTime lastUpdated = default);

        Task AddSubscriptionAsync(string userToken, Guid surveyId);
        Task RemoveSubscriptionAsync(string userToken, Guid surveyId);

        // Save changes
        Task<int> SaveChangesAsync();
        
    }
}
