using Lms.Api.DTOs.Quiz;

namespace Lms.Api.Services.Interfaces;

public interface IQuizService
{
    Task<List<QuizDto>> GetQuizzesByLessonAsync(Guid lessonId);
    Task<List<QuizAdminDto>> GetQuizzesByLessonAdminAsync(Guid lessonId);
    Task<QuizAdminDto> CreateQuizAsync(Guid lessonId, CreateQuizDto dto, Guid userId);
    Task<QuizAdminDto> UpdateQuizAsync(Guid quizId, CreateQuizDto dto, Guid userId);
    Task DeleteQuizAsync(Guid quizId, Guid userId);
    Task<QuizResultDto> SubmitQuizAsync(Guid userId, Guid lessonId, SubmitQuizDto dto);
    Task<QuizResultDto?> GetQuizResultAsync(Guid userId, Guid lessonId);
}
