using Lms.Api.Data;
using Lms.Api.DTOs.LessonProgress;
using Lms.Api.Entities;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Services;

public class LessonProgressService : ILessonProgressService
{
    private readonly AppDbContext _db;

    public LessonProgressService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LessonProgressDto> CompleteLessonAsync(Guid userId, Guid lessonId)
    {
        var lesson = await _db.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson is null)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");

        var enrolled = await _db.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == lesson.CourseId);
        if (!enrolled)
            throw new InvalidOperationException("You must be enrolled in this course.");

        var progress = await _db.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);

        if (progress is null)
        {
            progress = new LessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };
            _db.LessonProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return new LessonProgressDto
        {
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            OrderIndex = lesson.OrderIndex,
            IsCompleted = true,
            CompletedAt = progress.CompletedAt
        };
    }

    public async Task<CourseProgressDto> GetCourseProgressAsync(Guid userId, Guid courseId)
    {
        var course = await _db.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
            throw new KeyNotFoundException($"Course {courseId} not found.");

        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .Select(l => new
            {
                l.Id,
                l.Title,
                l.OrderIndex,
                Progress = _db.LessonProgresses
                    .FirstOrDefault(lp => lp.UserId == userId && lp.LessonId == l.Id)
            })
            .ToListAsync();

        return new CourseProgressDto
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            TotalLessons = lessons.Count,
            CompletedLessons = lessons.Count(l => l.Progress != null && l.Progress.IsCompleted),
            Lessons = lessons.Select(l => new LessonProgressDto
            {
                LessonId = l.Id,
                LessonTitle = l.Title,
                OrderIndex = l.OrderIndex,
                IsCompleted = l.Progress?.IsCompleted ?? false,
                CompletedAt = l.Progress?.CompletedAt
            }).ToList()
        };
    }
}
