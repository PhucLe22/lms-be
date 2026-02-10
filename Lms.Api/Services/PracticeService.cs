using Lms.Api.Data;
using Lms.Api.DTOs.Practice;
using Lms.Api.Entities;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Services;

public class PracticeService : IPracticeService
{
    private readonly AppDbContext _db;

    public PracticeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PracticeTaskDto>> GetTasksByLessonAsync(Guid lessonId)
    {
        var lessonExists = await _db.Lessons.AnyAsync(l => l.Id == lessonId);
        if (!lessonExists)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");

        return await _db.PracticeTasks
            .AsNoTracking()
            .Where(t => t.LessonId == lessonId)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new PracticeTaskDto
            {
                Id = t.Id,
                LessonId = t.LessonId,
                Title = t.Title,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PracticeTaskDto> CreateTaskAsync(Guid lessonId, CreatePracticeTaskDto dto, Guid userId)
    {
        var lesson = await _db.Lessons.Include(l => l.Course).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson is null)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");
        if (lesson.Course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only add practice tasks to your own courses.");

        var task = new PracticeTask
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            Title = dto.Title,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        _db.PracticeTasks.Add(task);
        await _db.SaveChangesAsync();

        return new PracticeTaskDto
        {
            Id = task.Id,
            LessonId = task.LessonId,
            Title = task.Title,
            Description = task.Description,
            CreatedAt = task.CreatedAt
        };
    }

    public async Task<PracticeTaskDto> UpdateTaskAsync(Guid taskId, CreatePracticeTaskDto dto, Guid userId)
    {
        var task = await _db.PracticeTasks
            .Include(t => t.Lesson).ThenInclude(l => l.Course)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null)
            throw new KeyNotFoundException($"Practice task {taskId} not found.");
        if (task.Lesson.Course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only update practice tasks in your own courses.");

        task.Title = dto.Title;
        task.Description = dto.Description;
        await _db.SaveChangesAsync();

        return new PracticeTaskDto
        {
            Id = task.Id,
            LessonId = task.LessonId,
            Title = task.Title,
            Description = task.Description,
            CreatedAt = task.CreatedAt
        };
    }

    public async Task DeleteTaskAsync(Guid taskId, Guid userId)
    {
        var task = await _db.PracticeTasks
            .Include(t => t.Lesson).ThenInclude(l => l.Course)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null)
            throw new KeyNotFoundException($"Practice task {taskId} not found.");
        if (task.Lesson.Course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only delete practice tasks in your own courses.");

        _db.PracticeTasks.Remove(task);
        await _db.SaveChangesAsync();
    }

    public async Task<PracticeSubmissionDto> SubmitAsync(Guid userId, Guid taskId, CreateSubmissionDto dto)
    {
        var task = await _db.PracticeTasks
            .Include(t => t.Lesson)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null)
            throw new KeyNotFoundException($"Practice task {taskId} not found.");

        var enrolled = await _db.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == task.Lesson.CourseId);
        if (!enrolled)
            throw new InvalidOperationException("You must be enrolled in this course.");

        var submission = new PracticeSubmission
        {
            Id = Guid.NewGuid(),
            PracticeTaskId = taskId,
            UserId = userId,
            SubmissionType = dto.SubmissionType,
            Content = dto.Content,
            SubmittedAt = DateTime.UtcNow
        };

        _db.PracticeSubmissions.Add(submission);
        await _db.SaveChangesAsync();

        var userName = await _db.Users.Where(u => u.Id == userId).Select(u => u.FullName).FirstAsync();

        return new PracticeSubmissionDto
        {
            Id = submission.Id,
            PracticeTaskId = submission.PracticeTaskId,
            SubmissionType = submission.SubmissionType,
            Content = submission.Content,
            SubmittedAt = submission.SubmittedAt,
            StudentName = userName
        };
    }

    public async Task<List<PracticeSubmissionDto>> GetMySubmissionsAsync(Guid userId, Guid taskId)
    {
        return await _db.PracticeSubmissions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.PracticeTaskId == taskId)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new PracticeSubmissionDto
            {
                Id = s.Id,
                PracticeTaskId = s.PracticeTaskId,
                SubmissionType = s.SubmissionType,
                Content = s.Content,
                SubmittedAt = s.SubmittedAt,
                StudentName = s.User.FullName
            })
            .ToListAsync();
    }

    public async Task<List<PracticeSubmissionDto>> GetAllSubmissionsAsync(Guid taskId)
    {
        return await _db.PracticeSubmissions
            .AsNoTracking()
            .Where(s => s.PracticeTaskId == taskId)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new PracticeSubmissionDto
            {
                Id = s.Id,
                PracticeTaskId = s.PracticeTaskId,
                SubmissionType = s.SubmissionType,
                Content = s.Content,
                SubmittedAt = s.SubmittedAt,
                StudentName = s.User.FullName
            })
            .ToListAsync();
    }
}
