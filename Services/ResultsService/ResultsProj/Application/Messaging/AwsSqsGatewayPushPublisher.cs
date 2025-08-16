using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using Application.Interfaces;
using Common;

namespace Infrastructure.Messaging
{
    public class AwsSqsGatewayPushPublisher : IGatewayPushPublisher
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;

        public AwsSqsGatewayPushPublisher(IAmazonSQS sqsClient, string queueUrl)
        {
            _sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
            _queueUrl = queueUrl ?? throw new ArgumentNullException(nameof(queueUrl));
        }

        public async Task<bool> PublishAsync(EventType eventType, string userToken, object payload)
        {
            var messageBody = JsonSerializer.Serialize(new
            {
                EventType = eventType,
                UserToken = userToken,
                Timestamp = DateTime.UtcNow,
                Payload = payload
            });

            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = messageBody
            };

            var response = await _sqsClient.SendMessageAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
