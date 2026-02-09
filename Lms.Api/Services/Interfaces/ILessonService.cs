using Lms.Api.DTOs.Lesson;

namespace Lms.Api.Services.Interfaces;

public interface ILessonService
{
    Task<List<LessonDto>> GetLessonsByCourseAsync(Guid courseId);
    Task<LessonDto> CreateLessonAsync(Guid courseId, CreateLessonDto dto, Guid userId);
    Task<LessonDto> UpdateLessonAsync(Guid lessonId, UpdateLessonDto dto, Guid userId);
    Task DeleteLessonAsync(Guid lessonId, Guid userId);
}
