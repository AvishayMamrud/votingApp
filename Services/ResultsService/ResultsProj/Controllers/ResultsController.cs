using Microsoft.AspNetCore.Mvc;
using System;
// using System.Threading.Tasks;
using Application.Interfaces;
using Amazon.Runtime; // For IDataAccess
// using Business.Models;        // For your result models

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly IResultsLogic _resultsLogic;

        public ResultsController(IResultsLogic resultsLogic)
        {
            _resultsLogic = resultsLogic ?? throw new ArgumentNullException(nameof(resultsLogic));
        }

        /// <summary>
        /// Get results for a specific survey
        /// </summary>
        [HttpGet("survey/{surveyId}")]
        public async Task<IActionResult> GetSurveyResults(
            [FromHeader(Name = "Authorization")] string authorization,
            Guid surveyId
        )
        {
            var userToken = getTokenFromAuthorizationHeader(authorization);
            if (string.IsNullOrEmpty(userToken))
                return Unauthorized("Missing or invalid token");

            var results = await _resultsLogic.GetSurveyResultsAsync(surveyId);
            if (results == null)
                return NotFound();

            return Ok(results);
        }

        /// <summary>
        /// Get results for a specific question
        /// </summary>
        [HttpGet("question/{questionId}")]
        public async Task<IActionResult> GetQuestionResults(
            [FromHeader(Name = "Authorization")] string authorization,
            Guid questionId
        )
        {
            var userToken = getTokenFromAuthorizationHeader(authorization);
            if (string.IsNullOrEmpty(userToken))
                return Unauthorized("Missing or invalid token");

            var result = await _resultsLogic.GetQuestionResultAsync(questionId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("survey/{surveyId}/live")]
        public async Task<IActionResult> GetSurveyResultsLive(
            [FromHeader(Name = "Authorization")] string authorization,
            Guid surveyId
        )
        {
            var userToken = getTokenFromAuthorizationHeader(authorization);
            if (string.IsNullOrEmpty(userToken))
                return Unauthorized("Missing or invalid token");

            _ = _resultsLogic.SubscribeSurveyAsync(userToken, surveyId);

            var results = await _resultsLogic.GetSurveyResultsAsync(surveyId);
            if (results == null)
                return NotFound();

            return Ok(results);
        }

        /// <summary>
        /// Get results for a specific question
        /// </summary>
        [HttpGet("question/{questionId}/live")]
        public async Task<IActionResult> GetQuestionResultsLive(
            [FromHeader(Name = "Authorization")] string authorization,
            Guid questionId
        )
        {
            var userToken = getTokenFromAuthorizationHeader(authorization);
            if (string.IsNullOrEmpty(userToken))
                return Unauthorized("Missing or invalid token");

            _ = _resultsLogic.SubscribeQuestionAsync(userToken, questionId);

            var result = await _resultsLogic.GetQuestionResultAsync(questionId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        private string? getTokenFromAuthorizationHeader(string authorization)
        {
            return authorization?.StartsWith("Bearer ") == true
                ? authorization.Substring("Bearer ".Length).Trim()
                : null;
        }
    }
}