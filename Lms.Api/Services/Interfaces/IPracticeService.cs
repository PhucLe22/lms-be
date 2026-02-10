using Lms.Api.DTOs.Practice;

namespace Lms.Api.Services.Interfaces;

public interface IPracticeService
{
    Task<List<PracticeTaskDto>> GetTasksByLessonAsync(Guid lessonId);
    Task<PracticeTaskDto> CreateTaskAsync(Guid lessonId, CreatePracticeTaskDto dto, Guid userId);
    Task<PracticeTaskDto> UpdateTaskAsync(Guid taskId, CreatePracticeTaskDto dto, Guid userId);
    Task DeleteTaskAsync(Guid taskId, Guid userId);
    Task<PracticeSubmissionDto> SubmitAsync(Guid userId, Guid taskId, CreateSubmissionDto dto);
    Task<List<PracticeSubmissionDto>> GetMySubmissionsAsync(Guid userId, Guid taskId);
    Task<List<PracticeSubmissionDto>> GetAllSubmissionsAsync(Guid taskId);
}
