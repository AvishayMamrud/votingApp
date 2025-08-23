using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Application.Interfaces;
using Data.DTOs;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging
{
    public class VoteResultsListener : IVoteResultsListener
    {
        private readonly IAmazonSQS _sqs;
        private readonly IResultsUpdateHandler _handler;
        private readonly string _queueUrl;

        public VoteResultsListener(IAmazonSQS sqs, IResultsUpdateHandler handler, IOptions<SqsOptions> options)
        {
            _sqs = sqs;
            _handler = handler;
            _queueUrl = options.Value.VotesQueueUrl ?? throw new ArgumentNullException(nameof(options), "SQS queue URL for votes is not configured.");
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
                        var voteDto = JsonSerializer.Deserialize<VoteDTO>(message.Body);
                        if (voteDto != null)
                        {
                            await _handler.HandleVoteUpdateAsync(voteDto);
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
