using Lms.Api.Data;
using Lms.Api.DTOs.Dashboard;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardDto> GetStudentDashboardAsync(Guid userId)
    {
        var enrollments = await _db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .ToListAsync();

        var completedLessonIds = await _db.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.UserId == userId && lp.IsCompleted)
            .Select(lp => lp.LessonId)
            .ToListAsync();

        var completedSet = completedLessonIds.ToHashSet();

        var courseSummaries = enrollments.Select(e =>
        {
            var totalLessons = e.Course.Lessons.Count;
            var completedLessons = e.Course.Lessons.Count(l => completedSet.Contains(l.Id));
            return new CourseProgressSummaryDto
            {
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                ProgressPercent = totalLessons > 0 ? (int)((double)completedLessons / totalLessons * 100) : 0
            };
        }).ToList();

        var totalLessonsAll = courseSummaries.Sum(c => c.TotalLessons);
        var completedLessonsAll = courseSummaries.Sum(c => c.CompletedLessons);
        var completedCourses = courseSummaries.Count(c => c.ProgressPercent == 100);

        var quizResults = await _db.QuizResults
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .ToListAsync();

        var avgScore = quizResults.Count > 0 ? quizResults.Average(r => r.Score) : 0;

        return new DashboardDto
        {
            TotalEnrolledCourses = enrollments.Count,
            CompletedCourses = completedCourses,
            TotalLessonsCompleted = completedLessonsAll,
            OverallProgressPercent = totalLessonsAll > 0
                ? Math.Round((double)completedLessonsAll / totalLessonsAll * 100, 1)
                : 0,
            AverageQuizScore = Math.Round(avgScore, 1),
            TotalQuizzesTaken = quizResults.Count,
            Courses = courseSummaries
        };
    }
}
