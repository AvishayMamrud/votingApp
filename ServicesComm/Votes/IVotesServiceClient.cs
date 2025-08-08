public interface IVotesServiceClient
{
    Task SubmitVoteBatchAsync(Guid userId, VoteBatchRequest request);
    Task<List<VoteDto>> GetVotesForUserAsync(Guid userId);
}
