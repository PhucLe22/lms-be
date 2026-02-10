using Lms.Api.DTOs.LessonProgress;

namespace Lms.Api.Services.Interfaces;

public interface ILessonProgressService
{
    Task<LessonProgressDto> CompleteLessonAsync(Guid userId, Guid lessonId);
    Task<LessonProgressDto> UpdateVideoProgressAsync(Guid userId, Guid lessonId, int watchPercent);
    Task<CourseProgressDto> GetCourseProgressAsync(Guid userId, Guid courseId);
}
