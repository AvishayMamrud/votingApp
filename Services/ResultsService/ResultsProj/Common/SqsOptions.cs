public class SqsOptions
{
    public string SurveysQueueUrl { get; set; } = string.Empty;
    public string VotesQueueUrl { get; set; } = string.Empty;
    public string GatewayPushQueueUrl { get; set; } = string.Empty;
}