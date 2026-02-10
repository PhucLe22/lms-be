using System.Security.Claims;
using Lms.Api.DTOs.Common;
using Lms.Api.DTOs.Dashboard;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lms.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Student")]
[EnableRateLimiting("authenticated")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dashboard = await _dashboardService.GetStudentDashboardAsync(userId);
        return Ok(ApiResponse<DashboardDto>.Ok(dashboard));
    }
}
