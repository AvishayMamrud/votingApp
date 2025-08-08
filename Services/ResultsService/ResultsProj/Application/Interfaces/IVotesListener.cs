namespace Application.Interfaces
{
    public interface IVoteResultsListener
    {
        Task StartListeningAsync(CancellationToken cancellationToken);
    }
}