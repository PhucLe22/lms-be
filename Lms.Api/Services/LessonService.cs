using Lms.Api.Data;
using Lms.Api.DTOs.Lesson;
using Lms.Api.Entities;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Services;

public class LessonService : ILessonService
{
    private readonly AppDbContext _db;

    public LessonService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LessonDto>> GetLessonsByCourseAsync(Guid courseId)
    {
        var courseExists = await _db.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
            throw new KeyNotFoundException($"Course {courseId} not found.");

        return await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonDto
            {
                Id = l.Id,
                Title = l.Title,
                Content = l.Content,
                VideoUrl = l.VideoUrl,
                DocumentUrl = l.DocumentUrl,
                OrderIndex = l.OrderIndex,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<LessonDto> CreateLessonAsync(Guid courseId, CreateLessonDto dto, Guid userId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course is null)
            throw new KeyNotFoundException($"Course {courseId} not found.");
        if (course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only add lessons to your own courses.");

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = dto.Title,
            Content = dto.Content,
            VideoUrl = dto.VideoUrl,
            DocumentUrl = dto.DocumentUrl,
            OrderIndex = dto.OrderIndex,
            CreatedAt = DateTime.UtcNow
        };

        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();

        return new LessonDto
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            DocumentUrl = lesson.DocumentUrl,
            OrderIndex = lesson.OrderIndex,
            CreatedAt = lesson.CreatedAt
        };
    }

    public async Task<LessonDto> UpdateLessonAsync(Guid lessonId, UpdateLessonDto dto, Guid userId)
    {
        var lesson = await _db.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson is null)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");
        if (lesson.Course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only update lessons in your own courses.");

        lesson.Title = dto.Title;
        lesson.Content = dto.Content;
        lesson.VideoUrl = dto.VideoUrl;
        lesson.DocumentUrl = dto.DocumentUrl;
        lesson.OrderIndex = dto.OrderIndex;
        await _db.SaveChangesAsync();

        return new LessonDto
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            DocumentUrl = lesson.DocumentUrl,
            OrderIndex = lesson.OrderIndex,
            CreatedAt = lesson.CreatedAt
        };
    }

    public async Task DeleteLessonAsync(Guid lessonId, Guid userId)
    {
        var lesson = await _db.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson is null)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");
        if (lesson.Course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only delete lessons in your own courses.");

        _db.Lessons.Remove(lesson);
        await _db.SaveChangesAsync();
    }
}
