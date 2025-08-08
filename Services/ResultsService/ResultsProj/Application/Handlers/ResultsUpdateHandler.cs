using Application.Interfaces;
using Data.DTOs;
using DAL.Entities;
using DAL.DbContext;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Data.ENUMs;



namespace Business.Logic
{
    public class ResultsUpdateHandler : IResultsUpdateHandler
    {
        private readonly ResultsDbContext _db;
        private readonly ILogger<ResultsUpdateHandler> _logger;

        public ResultsUpdateHandler(ResultsDbContext db, ILogger<ResultsUpdateHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task HandleSurveyUpdateAsync(SurveyDTO dto)
        {
            foreach (var question in dto.Questions)
            {
                var existing = await _db.QuestionResults
                    .FirstOrDefaultAsync(q => q.QuestionId == question.Id);

                if (existing == null)
                {
                    var qr = new QuestionResult
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        QuestionType = question.Type,
                        SurveyId = dto.Id,
                        SurveyTitle = dto.Title,
                        TotalAnswers = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    _db.QuestionResults.Add(qr);

                    if (question.Type == QuestionType.SingleChoice && question.Options != null)
                    {
                        foreach (var opt in question.Options)
                        {
                            _db.SingleChoiceResults.Add(new SingleChoiceResult
                            {
                                QuestionResultId = qr.QuestionId,
                                OptionId = opt.Id,
                                OptionText = opt.Text,
                                VoteCount = 0,
                            });
                        }
                    }
                    else if (question.Type == QuestionType.Range)
                    {
                        _db.RangeQuestionResults.Add(new RangeQuestionResult
                        {
                            QuestionResultId = qr.QuestionId,
                            AvgValue = 0,
                            StdDeviation = 0
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task HandleVoteUpdateAsync(VoteDTO vote)
        {
            var result = await _db.QuestionResults
                .FirstOrDefaultAsync(q => q.QuestionId == vote.QuestionId);

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
                    var opt = await _db.SingleChoiceResults
                        .FirstOrDefaultAsync(o => o.OptionId == vote.OptionId && o.QuestionResultId == result.QuestionId);

                    if (opt != null)
                    {
                        opt.VoteCount += 1;
                        _logger.LogDebug("Range result updated for question: {QuestionId}, Option: {OptionText}, VoteCount: {VoteCount}",
                            vote.QuestionId, opt.OptionText, opt.VoteCount);
                    }
                    else
                    {
                        _logger.LogWarning("SingleChioce result not found for (question, option): ({QuestionId}, {OptionId})",
                            vote.QuestionId, vote.OptionId);
                    }
                    break;

                case QuestionType.Range:
                    var range = await _db.RangeQuestionResults
                        .FirstOrDefaultAsync(r => r.QuestionResultId == result.QuestionId);

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

                        range.AvgValue = newMean;
                        range.StdDeviation = newStdDev;
                        _logger.LogDebug("Range result updated for question: {QuestionId}, New Avg: {Avg}, New StdDev: {StdDev}",
                            vote.QuestionId, newMean, newStdDev);
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

            await _db.SaveChangesAsync();
        }
    }
}
