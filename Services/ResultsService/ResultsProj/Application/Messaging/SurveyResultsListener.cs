using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Application.Interfaces;
using Data.DTOs;

namespace Application.Messaging
{
    public class SurveyResultsListener : ISurveyResultsListener
    {
        private readonly IAmazonSQS _sqs;
        private readonly IResultsUpdateHandler _handler;
        private readonly string _queueUrl;

        public SurveyResultsListener(IAmazonSQS sqs, IResultsUpdateHandler handler, IConfiguration config)
        {
            _sqs = sqs;
            _handler = handler;
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Configuration cannot be null");
            }
            _queueUrl = config["SQS:SurveyResultsQueueUrl"]!;
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20
                }, cancellationToken);

                foreach (var message in response.Messages)
                {
                    try
                    {
                        var surveyDto = JsonSerializer.Deserialize<SurveyDTO>(message.Body);
                        if (surveyDto != null)
                        {
                            await _handler.HandleSurveyUpdateAsync(surveyDto);
                        }
                        await _sqs.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
                    }
                    catch (Exception)
                    {
                        // Log or dead-letter
                    }
                }
            }
        }
    }
}
