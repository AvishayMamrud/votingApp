using Application.Interfaces;
using DAL.Entities;
using Data.DTOs;
using Data.ENUMs;

namespace Business.Logic
{
    public class ResultsLogic : IResultsLogic
    {
        private readonly IDataAccess _dataAccess;
        private readonly IGatewayPushPublisher _gatewayPushPublisher;
        private readonly ILiveUpdatesManager _liveUpdatesManager;
        private readonly ILogger<ResultsLogic> _logger;

        public ResultsLogic(IDataAccess dataAccess, IGatewayPushPublisher gatewayPushPublisher, ILiveUpdatesManager liveUpdatesManager, ILogger<ResultsLogic> logger)
        {
            _dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
            _gatewayPushPublisher = gatewayPushPublisher ?? throw new ArgumentNullException(nameof(gatewayPushPublisher));
            _liveUpdatesManager = liveUpdatesManager ?? throw new ArgumentNullException(nameof(liveUpdatesManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<QuestionResult?> GetQuestionResultAsync(Guid questionId)
        {
            return await _dataAccess.GetQuestionResultAsync(questionId);
        }

        public async Task<IReadOnlyList<QuestionResult>> GetSurveyResultsAsync(Guid surveyId)
        {
            return await _dataAccess.GetSurveyResultsAsync(surveyId);
        }

        public async Task AddQuestionResultAsync(Guid qid, string qtext, QuestionType qtype, Guid surveyId, string surveyTitle)
        {
            await _dataAccess.AddQuestionResultAsync(qid, qtext, qtype, surveyId, surveyTitle);
            await _dataAccess.SaveChangesAsync();
        }

        // public async Task PushVoteCountAsync(Guid questionId, Guid optionId, int newCount)
        // {
        //     await _gatewayPushPublisher.PublishAsync(EventType.VoteCountUpdated, new { QuestionId = questionId, OptionId = optionId, NewCount = newCount });
        // }

        // public async Task PushRangeStatsAsync(Guid questionId, double avg, double stdDev)
        // {
        //     await _gatewayPushPublisher.PublishAsync(EventType.RangeStatsUpdated, new { QuestionId = questionId, Avg = avg, StdDev = stdDev });
        // }

        public async Task<int> HandleSurveyUpdateAsync(SurveyDTO dto)
        {
            foreach (var question in dto.Questions)
            {
                await _dataAccess.AddQuestionResultAsync(dto.Id, dto.Title, question);
            }

            return await _dataAccess.SaveChangesAsync();
        }

        public async Task HandleVoteUpdateAsync(VoteDTO vote)
        {
            var result = await _dataAccess.GetQuestionResultAsync(vote.QuestionId);

            if (result == null)
            {
                _logger.LogWarning("Received vote for unknown question: {QuestionId}", vote.QuestionId);
                return;
            }

            result.TotalAnswers += 1;
            result.LastUpdated = DateTime.UtcNow;

            switch (result.QuestionType)
            {
                case QuestionType.SingleChoice:
                    var opt = result.SingleChoiceResults!.First(o => o.OptionId == vote.OptionId);

                    if (opt != null)
                    {
                        opt.VoteCount += 1;
                        _logger.LogDebug("Range result updated for question: {QuestionId}, Option: {OptionText}, VoteCount: {VoteCount}",
                            vote.QuestionId, opt.OptionText, opt.VoteCount + 1);

                        SingleChoiceResult? o = await _dataAccess.UpdateVoteCountAsync(vote.QuestionId, vote.OptionId, opt.VoteCount + 1);
                        if (o != null)
                        {
                            opt.VoteCount = o.VoteCount;
                            await _liveUpdatesManager.AddVoteUpdateAsync(vote.QuestionId, result);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("SingleChioce result not found for (question, option): ({QuestionId}, {OptionId})",
                            vote.QuestionId, vote.OptionId);
                    }
                    break;

                case QuestionType.Range:
                    var range = result.RangeResult;

                    if (range != null)
                    {
                        var existingVotes = result.TotalAnswers - 1;
                        var mean = range.AvgValue;
                        var newValue = vote.RangeVal;

                        // Welford's algorithm for running stddev
                        var delta = newValue - mean;
                        var newMean = mean + delta / result.TotalAnswers;
                        var newS = range.StdDeviation * range.StdDeviation * existingVotes +
                                    delta * (newValue - newMean);
                        var newStdDev = Math.Sqrt(newS / result.TotalAnswers);

                        _logger.LogDebug("Range result updated for question: {QuestionId}, New Avg: {Avg}, New StdDev: {StdDev}",
                            vote.QuestionId, newMean, newStdDev);

                        RangeQuestionResult? o = await _dataAccess.UpdateRangeStatsAsync(vote.QuestionId, newMean, newStdDev);
                        if (o != null)
                        {
                            range.AvgValue = o.AvgValue;
                            range.StdDeviation = o.StdDeviation;
                            await _liveUpdatesManager.AddVoteUpdateAsync(vote.QuestionId, result);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Range result not found for question: {QuestionId}", vote.QuestionId);
                    }
                    break;

                case QuestionType.OpenText:
                    // text results are not stored here
                    break;
            }

            await _dataAccess.SaveChangesAsync();
        }

        public async Task SubscribeSurveyAsync(string userToken, Guid surveyId)
        {
            foreach(var question in await _dataAccess.GetSurveyResultsAsync(surveyId))
            {
                await SubscribeQuestionAsync(userToken, question.QuestionId);
            }
        }

        public async Task SubscribeQuestionAsync(string userToken, Guid surveyId)
        {
            
            // await _dataAccess.AddSubscriptionAsync(userToken, surveyId);
            await _liveUpdatesManager.RegisterSubscriberAsync(surveyId, userToken);
        }

        public async Task UnsubscribeSurveyAsync(string userToken, Guid surveyId)
        {
            foreach(var question in await _dataAccess.GetSurveyResultsAsync(surveyId))
            {
                await UnsubscribeQuestionAsync(userToken, question.QuestionId);
            }
        }

        public async Task UnsubscribeQuestionAsync(string userToken, Guid surveyId)
        {
            // await _dataAccess.RemoveSubscriptionAsync(userToken, surveyId);
            await _liveUpdatesManager.RemoveSubscriberAsync(surveyId, userToken);
        }
    }
}