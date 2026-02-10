using Lms.Api.Data;
using Lms.Api.Entities;
using Lms.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Lms.Tests.Services;

public class LessonProgressServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LessonProgressService _sut;
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly Guid _lessonId = Guid.NewGuid();

    public LessonProgressServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        var adminId = Guid.NewGuid();
        _db.Users.AddRange(
            new User
            {
                Id = adminId, FullName = "Admin", Email = "admin@test.com",
                PasswordHash = "hash", Role = "Admin", CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = _studentId, FullName = "Student", Email = "student@test.com",
                PasswordHash = "hash", Role = "Student", CreatedAt = DateTime.UtcNow
            }
        );

        _db.Courses.Add(new Course
        {
            Id = _courseId, Title = "Test Course", Description = "Desc",
            CreatedBy = adminId, CreatedAt = DateTime.UtcNow
        });

        _db.Lessons.Add(new Lesson
        {
            Id = _lessonId, CourseId = _courseId, Title = "Lesson 1",
            Content = "Content", OrderIndex = 0, CreatedAt = DateTime.UtcNow
        });

        _db.Enrollments.Add(new Enrollment
        {
            Id = Guid.NewGuid(), UserId = _studentId, CourseId = _courseId,
            EnrolledAt = DateTime.UtcNow, Status = "Active"
        });

        _db.SaveChanges();

        _sut = new LessonProgressService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CompleteLessonAsync_Success_ReturnsProgress()
    {
        var result = await _sut.CompleteLessonAsync(_studentId, _lessonId);

        Assert.Equal(_lessonId, result.LessonId);
        Assert.Equal("Lesson 1", result.LessonTitle);
        Assert.True(result.IsCompleted);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task CompleteLessonAsync_AlreadyCompleted_UpdatesExisting()
    {
        _db.LessonProgresses.Add(new LessonProgress
        {
            Id = Guid.NewGuid(), UserId = _studentId, LessonId = _lessonId,
            IsCompleted = false, CompletedAt = null
        });
        await _db.SaveChangesAsync();

        var result = await _sut.CompleteLessonAsync(_studentId, _lessonId);

        Assert.True(result.IsCompleted);
        Assert.NotNull(result.CompletedAt);
        Assert.Equal(1, await _db.LessonProgresses.CountAsync());
    }

    [Fact]
    public async Task CompleteLessonAsync_LessonNotFound_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CompleteLessonAsync(_studentId, Guid.NewGuid()));
    }

    [Fact]
    public async Task CompleteLessonAsync_NotEnrolled_ThrowsInvalidOperation()
    {
        var otherStudentId = Guid.NewGuid();
        _db.Users.Add(new User
        {
            Id = otherStudentId, FullName = "Other", Email = "other@test.com",
            PasswordHash = "hash", Role = "Student", CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CompleteLessonAsync(otherStudentId, _lessonId));
    }

    [Fact]
    public async Task GetCourseProgressAsync_ReturnsCorrectProgress()
    {
        var lesson2Id = Guid.NewGuid();
        _db.Lessons.Add(new Lesson
        {
            Id = lesson2Id, CourseId = _courseId, Title = "Lesson 2",
            Content = "Content 2", OrderIndex = 1, CreatedAt = DateTime.UtcNow
        });

        _db.LessonProgresses.Add(new LessonProgress
        {
            Id = Guid.NewGuid(), UserId = _studentId, LessonId = _lessonId,
            IsCompleted = true, CompletedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetCourseProgressAsync(_studentId, _courseId);

        Assert.Equal("Test Course", result.CourseTitle);
        Assert.Equal(2, result.TotalLessons);
        Assert.Equal(1, result.CompletedLessons);
        Assert.Equal(50, result.ProgressPercent);
        Assert.Equal(2, result.Lessons.Count);
    }

    [Fact]
    public async Task GetCourseProgressAsync_CourseNotFound_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetCourseProgressAsync(_studentId, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetCourseProgressAsync_NoProgress_ReturnsZeroPercent()
    {
        var result = await _sut.GetCourseProgressAsync(_studentId, _courseId);

        Assert.Equal(0, result.CompletedLessons);
        Assert.Equal(0, result.ProgressPercent);
    }
}
