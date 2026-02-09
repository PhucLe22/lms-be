using System.Security.Claims;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lms.Api.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
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
        return Ok(result);
    }

    [HttpGet("api/courses/{courseId:guid}/progress")]
    public async Task<IActionResult> GetCourseProgress(Guid courseId)
    {
        var result = await _progressService.GetCourseProgressAsync(GetUserId(), courseId);
        return Ok(result);
    }
}
