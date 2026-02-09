using Lms.Api.Data;
using Lms.Api.DTOs.Course;
using Lms.Api.DTOs.Lesson;
using Lms.Api.Entities;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CourseDto>> GetAllCoursesAsync()
    {
        return await _db.Courses
            .AsNoTracking()
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                CreatorName = c.Creator.FullName,
                CreatedAt = c.CreatedAt,
                LessonCount = c.Lessons.Count,
                EnrollmentCount = c.Enrollments.Count
            })
            .ToListAsync();
    }

    public async Task<CourseDetailDto> GetCourseByIdAsync(Guid courseId)
    {
        var course = await _db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseDetailDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                CreatorName = c.Creator.FullName,
                CreatedAt = c.CreatedAt,
                Lessons = c.Lessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => new LessonDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Content = l.Content,
                        OrderIndex = l.OrderIndex,
                        CreatedAt = l.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (course is null)
            throw new KeyNotFoundException($"Course {courseId} not found.");

        return course;
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto, Guid creatorId)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            CreatedBy = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        var creatorName = await _db.Users
            .Where(u => u.Id == creatorId)
            .Select(u => u.FullName)
            .FirstAsync();

        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CreatorName = creatorName,
            CreatedAt = course.CreatedAt,
            LessonCount = 0,
            EnrollmentCount = 0
        };
    }

    public async Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourseDto dto, Guid userId)
    {
        var course = await _db.Courses
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
            throw new KeyNotFoundException($"Course {courseId} not found.");
        if (course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only update your own courses.");

        course.Title = dto.Title;
        course.Description = dto.Description;
        await _db.SaveChangesAsync();

        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CreatorName = course.Creator.FullName,
            CreatedAt = course.CreatedAt,
            LessonCount = await _db.Lessons.CountAsync(l => l.CourseId == courseId),
            EnrollmentCount = await _db.Enrollments.CountAsync(e => e.CourseId == courseId)
        };
    }

    public async Task DeleteCourseAsync(Guid courseId, Guid userId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course is null)
            throw new KeyNotFoundException($"Course {courseId} not found.");
        if (course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can only delete your own courses.");

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
    }
}
