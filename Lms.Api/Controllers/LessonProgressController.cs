using System.Security.Claims;
using Lms.Api.DTOs.Common;
using Lms.Api.DTOs.LessonProgress;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lms.Api.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
[EnableRateLimiting("authenticated")]
public class LessonProgressController : ControllerBase
{
    private readonly ILessonProgressService _progressService;

    public LessonProgressController(ILessonProgressService progressService)
    {
        _progressService = progressService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("api/lessons/{lessonId:guid}/complete")]
    public async Task<IActionResult> CompleteLesson(Guid lessonId)
    {
        var result = await _progressService.CompleteLessonAsync(GetUserId(), lessonId);
        return Ok(ApiResponse<LessonProgressDto>.Ok(result));
    }

    [HttpPut("api/lessons/{lessonId:guid}/watch-progress")]
    public async Task<IActionResult> UpdateVideoProgress(Guid lessonId, [FromBody] UpdateVideoProgressDto dto)
    {
        var result = await _progressService.UpdateVideoProgressAsync(GetUserId(), lessonId, dto.WatchPercent);
        return Ok(ApiResponse<LessonProgressDto>.Ok(result));
    }

    [HttpGet("api/courses/{courseId:guid}/progress")]
    public async Task<IActionResult> GetCourseProgress(Guid courseId)
    {
        var result = await _progressService.GetCourseProgressAsync(GetUserId(), courseId);
        return Ok(ApiResponse<CourseProgressDto>.Ok(result));
    }
}
