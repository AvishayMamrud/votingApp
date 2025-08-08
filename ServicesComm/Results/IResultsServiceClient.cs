public interface IResultsServiceClient
{
    Task<SurveyResultDto> GetResultsAsync(Guid surveyId);
}
