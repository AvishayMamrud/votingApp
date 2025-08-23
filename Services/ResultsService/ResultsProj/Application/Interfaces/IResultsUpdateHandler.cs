

using Data.DTOs;

namespace Application.Interfaces
{
    public interface IResultsUpdateHandler
    {
        Task<int> HandleSurveyUpdateAsync(SurveyDTO survey);
        Task HandleVoteUpdateAsync(VoteDTO vote);
    }
}