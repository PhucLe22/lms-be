using System.Security.Claims;
using Lms.Api.DTOs.Lesson;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lms.Api.Controllers;

[ApiController]
[Route("api/lessons")]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("/api/courses/{courseId:guid}/lessons")]
    public async Task<IActionResult> GetByCourse(Guid courseId)
    {
        var lessons = await _lessonService.GetLessonsByCourseAsync(courseId);
        return Ok(lessons);
    }

    [HttpPost("/api/courses/{courseId:guid}/lessons")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Guid courseId, [FromBody] CreateLessonDto dto)
    {
        var lesson = await _lessonService.CreateLessonAsync(courseId, dto, GetUserId());
        return Created($"api/lessons/{lesson.Id}", lesson);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonDto dto)
    {
        var lesson = await _lessonService.UpdateLessonAsync(id, dto, GetUserId());
        return Ok(lesson);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _lessonService.DeleteLessonAsync(id, GetUserId());
        return NoContent();
    }
}
