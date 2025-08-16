using Common;

namespace Application.Interfaces
{
    /// <summary>
    /// Sends events to SQS for the Gateway service to publish push notifications.
    /// </summary>
    public interface IGatewayPushPublisher
    {
        /// <summary>
        /// Publishes a push notification event to the Gateway's SQS queue.
        /// </summary>
        /// <param name="eventType">Type of the event (e.g., "VoteUpdated").</param>
        /// <param name="payload">Payload data to send in the event.</param>
        /// <returns>True if the message was sent successfully.</returns>
        Task<bool> PublishAsync(EventType eventType, string userToken, object payload);
    }
}