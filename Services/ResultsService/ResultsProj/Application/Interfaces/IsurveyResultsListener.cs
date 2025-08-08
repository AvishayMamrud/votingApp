namespace Application.Interfaces
{
    public interface ISurveyResultsListener
    {
        Task StartListeningAsync(CancellationToken cancellationToken);
    }
}