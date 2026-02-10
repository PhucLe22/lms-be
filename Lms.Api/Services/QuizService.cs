using Lms.Api.Data;
using Lms.Api.DTOs.Quiz;
using Lms.Api.Entities;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Services;

public class QuizService : IQuizService
{
    private readonly AppDbContext _db;

    public QuizService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<QuizDto>> GetQuizzesByLessonAsync(Guid lessonId)
    {
        var lessonExists = await _db.Lessons.AnyAsync(l => l.Id == lessonId);
        if (!lessonExists)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");

        return await _db.Quizzes
            .AsNoTracking()
            .Where(q => q.LessonId == lessonId)
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuizDto
            {
                Id = q.Id,
                Question = q.Question,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                OrderIndex = q.OrderIndex
            })
            .ToListAsync();
    }

    public async Task<List<QuizAdminDto>> GetQuizzesByLessonAdminAsync(Guid lessonId)
    {
        var lessonExists = await _db.Lessons.AnyAsync(l => l.Id == lessonId);
        if (!lessonExists)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");

        return await _db.Quizzes
            .AsNoTracking()
            .Where(q => q.LessonId == lessonId)
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuizAdminDto
            {
                Id = q.Id,
                Question = q.Question,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectAnswer,
                OrderIndex = q.OrderIndex
            })
            .ToListAsync();
    }

    public async Task<QuizAdminDto> CreateQuizAsync(Guid lessonId, CreateQuizDto dto, Guid userId)
    {
        var lesson = await _db.Lessons.Include(l => l.Course).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson is null)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");
        if (lesson.Course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only add quizzes to your own courses.");

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            Question = dto.Question,
            OptionA = dto.OptionA,
            OptionB = dto.OptionB,
            OptionC = dto.OptionC,
            OptionD = dto.OptionD,
            CorrectAnswer = dto.CorrectAnswer,
            OrderIndex = dto.OrderIndex,
            CreatedAt = DateTime.UtcNow
        };

        _db.Quizzes.Add(quiz);
        await _db.SaveChangesAsync();

        return new QuizAdminDto
        {
            Id = quiz.Id,
            Question = quiz.Question,
            OptionA = quiz.OptionA,
            OptionB = quiz.OptionB,
            OptionC = quiz.OptionC,
            OptionD = quiz.OptionD,
            CorrectAnswer = quiz.CorrectAnswer,
            OrderIndex = quiz.OrderIndex
        };
    }

    public async Task<QuizAdminDto> UpdateQuizAsync(Guid quizId, CreateQuizDto dto, Guid userId)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Lesson).ThenInclude(l => l.Course)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz is null)
            throw new KeyNotFoundException($"Quiz {quizId} not found.");
        if (quiz.Lesson.Course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only update quizzes in your own courses.");

        quiz.Question = dto.Question;
        quiz.OptionA = dto.OptionA;
        quiz.OptionB = dto.OptionB;
        quiz.OptionC = dto.OptionC;
        quiz.OptionD = dto.OptionD;
        quiz.CorrectAnswer = dto.CorrectAnswer;
        quiz.OrderIndex = dto.OrderIndex;
        await _db.SaveChangesAsync();

        return new QuizAdminDto
        {
            Id = quiz.Id,
            Question = quiz.Question,
            OptionA = quiz.OptionA,
            OptionB = quiz.OptionB,
            OptionC = quiz.OptionC,
            OptionD = quiz.OptionD,
            CorrectAnswer = quiz.CorrectAnswer,
            OrderIndex = quiz.OrderIndex
        };
    }

    public async Task DeleteQuizAsync(Guid quizId, Guid userId)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Lesson).ThenInclude(l => l.Course)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz is null)
            throw new KeyNotFoundException($"Quiz {quizId} not found.");
        if (quiz.Lesson.Course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only delete quizzes in your own courses.");

        _db.Quizzes.Remove(quiz);
        await _db.SaveChangesAsync();
    }

    public async Task<QuizResultDto> SubmitQuizAsync(Guid userId, Guid lessonId, SubmitQuizDto dto)
    {
        var lesson = await _db.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson is null)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");

        var enrolled = await _db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == lesson.CourseId);
        if (!enrolled)
            throw new InvalidOperationException("You must be enrolled in this course.");

        var quizzes = await _db.Quizzes
            .AsNoTracking()
            .Where(q => q.LessonId == lessonId)
            .ToListAsync();

        if (quizzes.Count == 0)
            throw new KeyNotFoundException("No quizzes found for this lesson.");

        var quizMap = quizzes.ToDictionary(q => q.Id);
        var correctCount = 0;

        foreach (var answer in dto.Answers)
        {
            if (quizMap.TryGetValue(answer.QuizId, out var quiz) && quiz.CorrectAnswer == answer.Answer)
                correctCount++;
        }

        var totalQuestions = quizzes.Count;
        var score = (int)((double)correctCount / totalQuestions * 100);

        var existing = await _db.QuizResults
            .FirstOrDefaultAsync(r => r.UserId == userId && r.LessonId == lessonId);

        if (existing is not null)
        {
            existing.TotalQuestions = totalQuestions;
            existing.CorrectAnswers = correctCount;
            existing.Score = score;
            existing.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new QuizResult
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctCount,
                Score = score,
                CompletedAt = DateTime.UtcNow
            };
            _db.QuizResults.Add(existing);
        }

        await _db.SaveChangesAsync();

        return new QuizResultDto
        {
            Id = existing.Id,
            LessonId = lessonId,
            TotalQuestions = totalQuestions,
            CorrectAnswers = correctCount,
            Score = score,
            CompletedAt = existing.CompletedAt
        };
    }

    public async Task<QuizResultDto?> GetQuizResultAsync(Guid userId, Guid lessonId)
    {
        var result = await _db.QuizResults
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.LessonId == lessonId);

        if (result is null) return null;

        return new QuizResultDto
        {
            Id = result.Id,
            LessonId = result.LessonId,
            TotalQuestions = result.TotalQuestions,
            CorrectAnswers = result.CorrectAnswers,
            Score = result.Score,
            CompletedAt = result.CompletedAt
        };
    }
}
