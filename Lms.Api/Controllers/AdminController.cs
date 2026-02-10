using Lms.Api.DTOs.Admin;
using Lms.Api.DTOs.Common;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lms.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetAllStudents(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _userService.GetAllUsersAsync(search, page, pageSize);
        return Ok(ApiResponse<PaginatedResult<StudentListDto>>.Ok(result));
    }

    [HttpGet("students/{id:guid}")]
    public async Task<IActionResult> GetStudent(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(ApiResponse<StudentDetailDto>.Ok(result));
    }

    [HttpDelete("students/{id:guid}")]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }

    [HttpPut("students/{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var result = await _userService.UpdateRoleAsync(id, dto);
        return Ok(ApiResponse<StudentListDto>.Ok(result));
    }
}
