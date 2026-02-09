using Lms.Api.DTOs.Course;

namespace Lms.Api.Services.Interfaces;

public interface ICourseService
{
    Task<List<CourseDto>> GetAllCoursesAsync();
    Task<CourseDetailDto> GetCourseByIdAsync(Guid courseId);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto dto, Guid creatorId);
    Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourseDto dto, Guid userId);
    Task DeleteCourseAsync(Guid courseId, Guid userId);
}
