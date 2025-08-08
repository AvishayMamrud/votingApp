

using Data.DTOs;

namespace Application.Interfaces
{
    public interface IResultsUpdateHandler
    {
        Task HandleSurveyUpdateAsync(SurveyDTO survey);
        Task HandleVoteUpdateAsync(VoteDTO vote);
    }
}