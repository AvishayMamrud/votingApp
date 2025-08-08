public interface ISurveysServiceClient
{
    Task<SurveyDto> GetSurveyAsync(Guid surveyId);
    Task<List<SurveyDto>> GetAllSurveysAsync();
    Task SubmitSurveyAsync(Guid userId, SubmitSurveyRequest request);
}
