using System.Security.Claims;
using Lms.Api.DTOs.Common;
using Lms.Api.DTOs.Quiz;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lms.Api.Controllers;

[ApiController]
[Route("api")]
[EnableRateLimiting("authenticated")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("lessons/{lessonId:guid}/quiz")]
    [Authorize]
    public async Task<IActionResult> GetQuizzes(Guid lessonId)
    {
        var quizzes = await _quizService.GetQuizzesByLessonAsync(lessonId);
        return Ok(ApiResponse<List<QuizDto>>.Ok(quizzes));
    }

    [HttpGet("lessons/{lessonId:guid}/quiz/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetQuizzesAdmin(Guid lessonId)
    {
        var quizzes = await _quizService.GetQuizzesByLessonAdminAsync(lessonId);
        return Ok(ApiResponse<List<QuizAdminDto>>.Ok(quizzes));
    }

    [HttpPost("lessons/{lessonId:guid}/quiz")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuiz(Guid lessonId, [FromBody] CreateQuizDto dto)
    {
        var quiz = await _quizService.CreateQuizAsync(lessonId, dto, GetUserId());
        return Created($"api/quiz/{quiz.Id}", ApiResponse<QuizAdminDto>.Ok(quiz));
    }

    [HttpPut("quiz/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateQuiz(Guid id, [FromBody] CreateQuizDto dto)
    {
        var quiz = await _quizService.UpdateQuizAsync(id, dto, GetUserId());
        return Ok(ApiResponse<QuizAdminDto>.Ok(quiz));
    }

    [HttpDelete("quiz/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteQuiz(Guid id)
    {
        await _quizService.DeleteQuizAsync(id, GetUserId());
        return NoContent();
    }

    [HttpPost("quiz/submit/{lessonId:guid}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitQuiz(Guid lessonId, [FromBody] SubmitQuizDto dto)
    {
        var result = await _quizService.SubmitQuizAsync(GetUserId(), lessonId, dto);
        return Ok(ApiResponse<QuizResultDto>.Ok(result));
    }

    [HttpGet("quiz/result/{lessonId:guid}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetQuizResult(Guid lessonId)
    {
        var result = await _quizService.GetQuizResultAsync(GetUserId(), lessonId);
        return Ok(ApiResponse<QuizResultDto?>.Ok(result));
    }
}
