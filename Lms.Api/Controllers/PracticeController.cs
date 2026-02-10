using System.Security.Claims;
using Lms.Api.DTOs.Common;
using Lms.Api.DTOs.Practice;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lms.Api.Controllers;

[ApiController]
[Route("api")]
[EnableRateLimiting("authenticated")]
public class PracticeController : ControllerBase
{
    private readonly IPracticeService _practiceService;

    public PracticeController(IPracticeService practiceService)
    {
        _practiceService = practiceService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("lessons/{lessonId:guid}/practice")]
    [Authorize]
    public async Task<IActionResult> GetTasks(Guid lessonId)
    {
        var tasks = await _practiceService.GetTasksByLessonAsync(lessonId);
        return Ok(ApiResponse<List<PracticeTaskDto>>.Ok(tasks));
    }

    [HttpPost("lessons/{lessonId:guid}/practice")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTask(Guid lessonId, [FromBody] CreatePracticeTaskDto dto)
    {
        var task = await _practiceService.CreateTaskAsync(lessonId, dto, GetUserId());
        return Created($"api/practice/{task.Id}", ApiResponse<PracticeTaskDto>.Ok(task));
    }

    [HttpPut("practice/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] CreatePracticeTaskDto dto)
    {
        var task = await _practiceService.UpdateTaskAsync(id, dto, GetUserId());
        return Ok(ApiResponse<PracticeTaskDto>.Ok(task));
    }

    [HttpDelete("practice/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        await _practiceService.DeleteTaskAsync(id, GetUserId());
        return NoContent();
    }

    [HttpPost("practice/{taskId:guid}/submit")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Submit(Guid taskId, [FromBody] CreateSubmissionDto dto)
    {
        var submission = await _practiceService.SubmitAsync(GetUserId(), taskId, dto);
        return Ok(ApiResponse<PracticeSubmissionDto>.Ok(submission));
    }

    [HttpGet("practice/{taskId:guid}/my-submissions")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMySubmissions(Guid taskId)
    {
        var submissions = await _practiceService.GetMySubmissionsAsync(GetUserId(), taskId);
        return Ok(ApiResponse<List<PracticeSubmissionDto>>.Ok(submissions));
    }

    [HttpGet("practice/{taskId:guid}/submissions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllSubmissions(Guid taskId)
    {
        var submissions = await _practiceService.GetAllSubmissionsAsync(taskId);
        return Ok(ApiResponse<List<PracticeSubmissionDto>>.Ok(submissions));
    }
}
