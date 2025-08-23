using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Application.Interfaces;
using Data.DTOs;
using Microsoft.Extensions.Options;

namespace Application.Messaging
{
    public class SurveyResultsListener : ISurveyResultsListener
    {
        private readonly IAmazonSQS _sqs;
        private readonly IResultsUpdateHandler _handler;
        private readonly string _queueUrl;

        public SurveyResultsListener(IAmazonSQS sqs, IResultsUpdateHandler handler, IOptions<SqsOptions> options)
        {
            _sqs = sqs;
            _handler = handler;
            _queueUrl = options.Value.SurveysQueueUrl ?? throw new ArgumentNullException(nameof(options), "SQS queue URL for surveys is not configured.");
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
