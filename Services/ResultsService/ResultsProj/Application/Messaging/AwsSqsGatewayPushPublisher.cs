using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using Application.Interfaces;
using Common;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging
{
    public class AwsSqsGatewayPushPublisher : IGatewayPushPublisher
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;

        public AwsSqsGatewayPushPublisher(IAmazonSQS sqsClient, IOptions<SqsOptions> options)
        {
            _sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
            _queueUrl = options.Value.GatewayPushQueueUrl 
                ?? throw new ArgumentNullException(nameof(options), "SQS queue URL for gateway push is not configured.");
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
