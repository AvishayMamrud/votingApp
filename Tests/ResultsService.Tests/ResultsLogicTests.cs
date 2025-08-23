using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Application.Interfaces;
using Data.DTOs;
using DAL.Entities;
using Business.Logic;
using Microsoft.Extensions.Logging;
using Data.ENUMs;

namespace ResultsService.Tests
{
    public class ResultsLogicTests
    {
        private readonly Mock<IDataAccess> _dataAccessMock;
        private readonly Mock<IGatewayPushPublisher> _gatewayMock;
        private readonly Mock<ILiveUpdatesManager> _liveUpdatesMock;
        private readonly Mock<ILogger<ResultsLogic>> _loggerMock;
        private readonly ResultsLogic _logic;

        public ResultsLogicTests()
        {
            _dataAccessMock = new Mock<IDataAccess>(MockBehavior.Strict);
            _gatewayMock = new Mock<IGatewayPushPublisher>(MockBehavior.Strict);
            _liveUpdatesMock = new Mock<ILiveUpdatesManager>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<ResultsLogic>>();

            _logic = new ResultsLogic(
                _dataAccessMock.Object,
                _gatewayMock.Object,
                _liveUpdatesMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task HandleSurveyUpdateAsync_Should_AddQuestions_And_Save()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var q1 = new QuestionDTO(Guid.NewGuid(), "Q1", QuestionType.SingleChoice,
                new List<OptionDTO> { new OptionDTO(Guid.NewGuid(), "opt", 0) }, 0, 0, true);
            var dto = new SurveyDTO(surveyId, "Survey", new List<QuestionDTO> { q1 });

            _dataAccessMock
                .Setup(d => d.AddQuestionResultAsync(surveyId, "Survey", q1))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _dataAccessMock
                .Setup(d => d.SaveChangesAsync())
                .ReturnsAsync(2)
                .Verifiable();

            // Act
            var result = await _logic.HandleSurveyUpdateAsync(dto);

            // Assert
            Assert.Equal(2, result);
            _dataAccessMock.VerifyAll();
        }

        [Fact]
        public async Task HandleVoteUpdateAsync_Should_LogWarning_When_QuestionNotFound()
        {
            // Arrange
            var vote = new VoteDTO(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, QuestionType.Range, default, 3);
            _dataAccessMock
                .Setup(d => d.GetQuestionResultAsync(vote.QuestionId))
                .ReturnsAsync((QuestionResult?)null);

            // Act
            await _logic.HandleVoteUpdateAsync(vote);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Received vote for unknown question")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleVoteUpdateAsync_Should_Update_SingleChoice()
        {
            // Arrange
            var qid = Guid.NewGuid();
            var oid = Guid.NewGuid();

            var result = new QuestionResult
            {
                QuestionId = qid,
                QuestionType = QuestionType.SingleChoice,
                TotalAnswers = 0,
                SingleChoiceResults = new List<SingleChoiceResult>
                {
                    new SingleChoiceResult { QuestionResultId = qid, OptionId = oid, OptionText = "O1", VoteCount = 0 }
                }
            };

            var vote = new VoteDTO(qid, Guid.NewGuid(), Guid.NewGuid(), qid, QuestionType.SingleChoice, oid);

            _dataAccessMock.Setup(d => d.GetQuestionResultAsync(qid)).ReturnsAsync(result);
            _dataAccessMock.Setup(d => d.UpdateVoteCountAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync(new SingleChoiceResult { VoteCount = 22 }).Verifiable();
            _dataAccessMock.Setup(d => d.SaveChangesAsync()).ReturnsAsync(1).Verifiable();
            _liveUpdatesMock.Setup(x => x.AddVoteUpdateAsync(It.IsAny<Guid>(), It.IsAny<QuestionResult>()))
                .Returns(Task.CompletedTask).Verifiable();

            // Act
            await _logic.HandleVoteUpdateAsync(vote);

            // Assert
            Assert.Equal(22, result.SingleChoiceResults.First().VoteCount);
            Assert.Equal(1, result.TotalAnswers);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Range result updated for question")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _dataAccessMock.VerifyAll();
            _liveUpdatesMock.VerifyAll();
        }

        [Fact]
        public async Task HandleVoteUpdateAsync_Should_Update_Range()
        {
            // Arrange
            var qid = Guid.NewGuid();
            var result = new QuestionResult
            {
                QuestionId = qid,
                QuestionType = QuestionType.Range,
                TotalAnswers = 0,
                RangeResult = new RangeQuestionResult { QuestionResultId = qid, AvgValue = 0, StdDeviation = 0 }
            };

            var vote = new VoteDTO(qid, Guid.NewGuid(), Guid.NewGuid(), qid, QuestionType.Range) { RangeVal = 3 };

            _dataAccessMock.Setup(d => d.GetQuestionResultAsync(qid)).ReturnsAsync(result);
            _dataAccessMock.Setup(d => d.UpdateRangeStatsAsync(qid, It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync(new RangeQuestionResult {AvgValue = 0.1, StdDeviation = 0.5}).Verifiable();
            _dataAccessMock.Setup(d => d.SaveChangesAsync()).ReturnsAsync(1).Verifiable();

            _liveUpdatesMock.Setup(x => x.AddVoteUpdateAsync(It.IsAny<Guid>(), It.IsAny<QuestionResult>()))
                .Returns(Task.CompletedTask).Verifiable();

            // Act
            await _logic.HandleVoteUpdateAsync(vote);

            // Assert
            Assert.Equal(1, result.TotalAnswers);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Range result updated for question")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            _dataAccessMock.VerifyAll();
            _liveUpdatesMock.VerifyAll();
        }

        [Fact]
        public async Task SubscribeSurveyAsync_Should_RegisterAllQuestions()
        {
            // Arrange
            var surveyId = Guid.NewGuid();
            var userToken = "abc";

            var questions = new List<QuestionResult>
            {
                new QuestionResult { QuestionId = Guid.NewGuid() },
                new QuestionResult { QuestionId = Guid.NewGuid() }
            };

            _dataAccessMock.Setup(d => d.GetSurveyResultsAsync(surveyId)).ReturnsAsync(questions);
            foreach (var q in questions)
            {
                _liveUpdatesMock.Setup(m => m.RegisterSubscriberAsync(q.QuestionId, userToken)).Returns(Task.CompletedTask).Verifiable();
            }

            // Act
            await _logic.SubscribeSurveyAsync(userToken, surveyId);

            // Assert
            _liveUpdatesMock.VerifyAll();
        }

        [Fact]
        public async Task UnsubscribeSurveyAsync_Should_RemoveAllQuestions()
        {
            var surveyId = Guid.NewGuid();
            var userToken = "abc";

            var questions = new List<QuestionResult>
            {
                new QuestionResult { QuestionId = Guid.NewGuid() },
                new QuestionResult { QuestionId = Guid.NewGuid() }
            };

            _dataAccessMock.Setup(d => d.GetSurveyResultsAsync(surveyId)).ReturnsAsync(questions);
            foreach (var q in questions)
            {
                _liveUpdatesMock.Setup(m => m.RemoveSubscriberAsync(q.QuestionId, userToken)).Returns(Task.CompletedTask).Verifiable();
            }

            // Act
            await _logic.UnsubscribeSurveyAsync(userToken, surveyId);

            // Assert
            _liveUpdatesMock.VerifyAll();
        }

        [Fact]
        public async Task GetQuestionResultAsync_Should_Return_FromDataAccess()
        {
            var qid = Guid.NewGuid();
            var qr = new QuestionResult { QuestionId = qid };
            _dataAccessMock.Setup(d => d.GetQuestionResultAsync(qid)).ReturnsAsync(qr);

            var result = await _logic.GetQuestionResultAsync(qid);

            Assert.Equal(qid, result!.QuestionId);
        }

        [Fact]
        public async Task GetSurveyResultsAsync_Should_Return_FromDataAccess()
        {
            var sid = Guid.NewGuid();
            var qrs = new List<QuestionResult> { new QuestionResult(), new QuestionResult() };
            _dataAccessMock.Setup(d => d.GetSurveyResultsAsync(sid)).ReturnsAsync(qrs);

            var result = await _logic.GetSurveyResultsAsync(sid);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddQuestionResultAsync_Should_Add_And_Save()
        {
            var qid = Guid.NewGuid();
            var sid = Guid.NewGuid();

            _dataAccessMock.Setup(d => d.AddQuestionResultAsync(qid, "q", QuestionType.SingleChoice, sid, "survey", 0, default))
                .Returns(Task.CompletedTask).Verifiable();
            _dataAccessMock.Setup(d => d.SaveChangesAsync()).ReturnsAsync(1).Verifiable();

            await _logic.AddQuestionResultAsync(qid, "q", QuestionType.SingleChoice, sid, "survey");

            _dataAccessMock.VerifyAll();
        }
    }
}
