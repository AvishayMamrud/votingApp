using Application.Interfaces;
using Common;

namespace Infrastructure
{
    /// <summary>
    /// Background service that polls for live updates and publishes them to the Gateway service.
    /// </summary>
    public class LiveUpdatesBackgroundService : BackgroundService
    {
        private readonly ILiveUpdatesManager _liveUpdatesManager;
        private readonly IGatewayPushPublisher _pushPublisher;
        private readonly ILogger<LiveUpdatesBackgroundService> _logger;

        public LiveUpdatesBackgroundService(
            ILiveUpdatesManager liveUpdatesManager,
            IGatewayPushPublisher pushPublisher,
            ILogger<LiveUpdatesBackgroundService> logger)
        {
            _liveUpdatesManager = liveUpdatesManager;
            _pushPublisher = pushPublisher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Live Updates Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var updates = await _liveUpdatesManager.PollUpdatesAsync();

                foreach (var (userToken, update) in updates)
                {
                    try
                    {
                        await _pushPublisher.PublishAsync(EventType.QuestionResultUpdated, userToken, update);
                        _logger.LogInformation($"Pushed update to user {userToken}.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to push update to user {userToken}.");
                    }
                }

                // Delay to prevent tight loop; adjust as needed
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Live Updates Background Service is stopping.");
        }
    }
}