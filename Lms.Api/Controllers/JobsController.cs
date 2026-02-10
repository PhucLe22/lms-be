using Hangfire;
using Lms.Api.DTOs.Common;
using Lms.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lms.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("admin")]
public class JobsController : ControllerBase
{
    private readonly IBackgroundJobClient _jobClient;

    public JobsController(IBackgroundJobClient jobClient)
    {
        _jobClient = jobClient;
    }

    [HttpPost("test")]
    public IActionResult EnqueueTestJob()
    {
        var jobId = _jobClient.Enqueue<BackgroundJobService>(
            x => x.SendCourseCompletedEmail("test@example.com", "Demo Course"));

        return Ok(ApiResponse<object>.Ok(
            new { JobId = jobId },
            "Background job enqueued successfully. Check /hangfire dashboard."));
    }
}
