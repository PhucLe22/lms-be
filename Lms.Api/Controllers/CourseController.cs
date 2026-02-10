using System.Security.Claims;
using Lms.Api.DTOs.Common;
using Lms.Api.DTOs.Course;
using Lms.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lms.Api.Controllers;

[ApiController]
[Route("api/courses")]
[EnableRateLimiting("public")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? level,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _courseService.GetAllCoursesAsync(search, level, page, pageSize);
        return Ok(ApiResponse<PaginatedResult<CourseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        return Ok(ApiResponse<CourseDetailDto>.Ok(course));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
    {
        var course = await _courseService.CreateCourseAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, ApiResponse<CourseDto>.Ok(course));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto)
    {
        var course = await _courseService.UpdateCourseAsync(id, dto, GetUserId());
        return Ok(ApiResponse<CourseDto>.Ok(course));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _courseService.DeleteCourseAsync(id, GetUserId());
        return NoContent();
    }
}
