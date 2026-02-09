using System.Security.Claims;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lms.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize(Roles = "Student")]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("{courseId:guid}")]
    public async Task<IActionResult> Enroll(Guid courseId)
    {
        try
        {
            var enrollment = await _enrollmentService.EnrollAsync(GetUserId(), courseId);
            return Created($"api/enrollments/{enrollment.Id}", enrollment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(GetUserId());
        return Ok(enrollments);
    }

    [HttpDelete("{courseId:guid}")]
    public async Task<IActionResult> Unenroll(Guid courseId)
    {
        try
        {
            await _enrollmentService.UnenrollAsync(GetUserId(), courseId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
