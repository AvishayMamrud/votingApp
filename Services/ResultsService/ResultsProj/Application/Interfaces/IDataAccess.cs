using DAL.Entities;

namespace ResultsService.Application.Interfaces
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
        Task AddQuestionResultAsync(QuestionResult result);
        Task UpdateVoteCountAsync(Guid questionId, Guid optionId, int newCount);
        Task UpdateRangeStatsAsync(Guid questionId, double avg, double stdDev);

        // Save changes
        Task<int> SaveChangesAsync();
    }
}
