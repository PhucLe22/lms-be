using System.Security.Claims;
using Lms.Api.DTOs.Common;
using Lms.Api.DTOs.Enrollment;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lms.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize(Roles = "Student")]
[EnableRateLimiting("authenticated")]
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
        var enrollment = await _enrollmentService.EnrollAsync(GetUserId(), courseId);
        return Created($"api/enrollments/{enrollment.Id}", ApiResponse<EnrollmentDto>.Ok(enrollment));
    }

    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(GetUserId());
        return Ok(ApiResponse<List<EnrollmentDto>>.Ok(enrollments));
    }

    [HttpDelete("{courseId:guid}")]
    public async Task<IActionResult> Unenroll(Guid courseId)
    {
        await _enrollmentService.UnenrollAsync(GetUserId(), courseId);
        return NoContent();
    }
}
