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

        // Check video watch requirement (>= 80%)
        if (!string.IsNullOrEmpty(lesson.VideoUrl))
        {
            var existingProgress = await _db.LessonProgresses
                .AsNoTracking()
                .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);

            var watchPercent = existingProgress?.VideoWatchPercent ?? 0;
            if (watchPercent < 80)
                throw new InvalidOperationException(
                    $"You must watch at least 80% of the video before completing this lesson. Current: {watchPercent}%.");
        }

        // Check quiz completion requirement
        var hasQuizzes = await _db.Quizzes.AnyAsync(q => q.LessonId == lessonId);
        if (hasQuizzes)
        {
            var hasQuizResult = await _db.QuizResults
                .AnyAsync(qr => qr.UserId == userId && qr.LessonId == lessonId);
            if (!hasQuizResult)
                throw new InvalidOperationException(
                    "You must complete the quiz before marking this lesson as completed.");
        }

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
            VideoWatchPercent = progress.VideoWatchPercent,
            IsCompleted = true,
            CompletedAt = progress.CompletedAt
        };
    }

    public async Task<LessonProgressDto> UncompleteLessonAsync(Guid userId, Guid lessonId)
    {
        var lesson = await _db.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson is null)
            throw new KeyNotFoundException($"Lesson {lessonId} not found.");

        var progress = await _db.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);

        if (progress is null || !progress.IsCompleted)
            throw new InvalidOperationException("This lesson is not marked as completed.");

        progress.IsCompleted = false;
        progress.CompletedAt = null;

        await _db.SaveChangesAsync();

        return new LessonProgressDto
        {
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            OrderIndex = lesson.OrderIndex,
            VideoWatchPercent = progress.VideoWatchPercent,
            IsCompleted = false,
            CompletedAt = null
        };
    }

    public async Task<LessonProgressDto> UpdateVideoProgressAsync(Guid userId, Guid lessonId, int watchPercent)
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

        watchPercent = Math.Clamp(watchPercent, 0, 100);

        var progress = await _db.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);

        if (progress is null)
        {
            progress = new LessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                VideoWatchPercent = watchPercent
            };
            _db.LessonProgresses.Add(progress);
        }
        else
        {
            // Only allow increasing watch percent (prevent cheating by resending lower values)
            if (watchPercent > progress.VideoWatchPercent)
                progress.VideoWatchPercent = watchPercent;
        }

        await _db.SaveChangesAsync();

        return new LessonProgressDto
        {
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            OrderIndex = lesson.OrderIndex,
            VideoWatchPercent = progress.VideoWatchPercent,
            IsCompleted = progress.IsCompleted,
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
                VideoWatchPercent = l.Progress?.VideoWatchPercent ?? 0,
                IsCompleted = l.Progress?.IsCompleted ?? false,
                CompletedAt = l.Progress?.CompletedAt
            }).ToList()
        };
    }
}
