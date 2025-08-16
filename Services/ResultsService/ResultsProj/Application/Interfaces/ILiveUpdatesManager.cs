using DAL.Entities;

namespace Application.Interfaces
{
    public interface ILiveUpdatesManager
    {
        Task RegisterSubscriberAsync(Guid questionId, string userToken);
        Task RemoveSubscriberAsync(Guid questionId, string userToken);
        Task AddVoteUpdateAsync(Guid questionId, QuestionResult updatePayload);
        Task<IEnumerable<(string userToken, QuestionResult Update)>> PollUpdatesAsync();
    }
}