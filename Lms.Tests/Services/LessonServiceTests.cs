using Lms.Api.Data;
using Lms.Api.DTOs.Lesson;
using Lms.Api.Entities;
using Lms.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Lms.Tests.Services;

public class LessonServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LessonService _sut;
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();

    public LessonServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        _db.Users.Add(new User
        {
            Id = _adminId, FullName = "Admin", Email = "admin@test.com",
            PasswordHash = "hash", Role = "Admin", CreatedAt = DateTime.UtcNow
        });

        _db.Courses.Add(new Course
        {
            Id = _courseId, Title = "Test Course", Description = "Desc",
            CreatedBy = _adminId, CreatedAt = DateTime.UtcNow
        });
        _db.SaveChanges();

        _sut = new LessonService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateLessonAsync_Success_ReturnsLessonDto()
    {
        var dto = new CreateLessonDto { Title = "Intro", Content = "Hello", OrderIndex = 0 };

        var result = await _sut.CreateLessonAsync(_courseId, dto, _adminId);

        Assert.Equal("Intro", result.Title);
        Assert.Equal("Hello", result.Content);
        Assert.Equal(0, result.OrderIndex);
    }

    [Fact]
    public async Task CreateLessonAsync_CourseNotFound_ThrowsKeyNotFound()
    {
        var dto = new CreateLessonDto { Title = "Intro", Content = "Hello", OrderIndex = 0 };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateLessonAsync(Guid.NewGuid(), dto, _adminId));
    }

    [Fact]
    public async Task CreateLessonAsync_NotOwner_ThrowsUnauthorized()
    {
        var dto = new CreateLessonDto { Title = "Intro", Content = "Hello", OrderIndex = 0 };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.CreateLessonAsync(_courseId, dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetLessonsByCourseAsync_ReturnsOrderedLessons()
    {
        _db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = _courseId, Title = "Second",
                Content = "B", OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = _courseId, Title = "First",
                Content = "A", OrderIndex = 0, CreatedAt = DateTime.UtcNow
            }
        );
        await _db.SaveChangesAsync();

        var result = await _sut.GetLessonsByCourseAsync(_courseId);

        Assert.Equal(2, result.Count);
        Assert.Equal("First", result[0].Title);
        Assert.Equal("Second", result[1].Title);
    }

    [Fact]
    public async Task GetLessonsByCourseAsync_CourseNotFound_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetLessonsByCourseAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateLessonAsync_Success_ReturnsUpdated()
    {
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(), CourseId = _courseId, Title = "Old",
            Content = "Old Content", OrderIndex = 0, CreatedAt = DateTime.UtcNow
        };
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();

        var dto = new UpdateLessonDto { Title = "New", Content = "New Content", OrderIndex = 1 };

        var result = await _sut.UpdateLessonAsync(lesson.Id, dto, _adminId);

        Assert.Equal("New", result.Title);
        Assert.Equal("New Content", result.Content);
        Assert.Equal(1, result.OrderIndex);
    }

    [Fact]
    public async Task UpdateLessonAsync_NotOwner_ThrowsUnauthorized()
    {
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(), CourseId = _courseId, Title = "Lesson",
            Content = "Content", OrderIndex = 0, CreatedAt = DateTime.UtcNow
        };
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();

        var dto = new UpdateLessonDto { Title = "Hack", Content = "Hacked", OrderIndex = 0 };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.UpdateLessonAsync(lesson.Id, dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteLessonAsync_Success_RemovesFromDb()
    {
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(), CourseId = _courseId, Title = "Delete Me",
            Content = "Bye", OrderIndex = 0, CreatedAt = DateTime.UtcNow
        };
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();

        await _sut.DeleteLessonAsync(lesson.Id, _adminId);

        Assert.False(await _db.Lessons.AnyAsync(l => l.Id == lesson.Id));
    }

    [Fact]
    public async Task DeleteLessonAsync_NotOwner_ThrowsUnauthorized()
    {
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(), CourseId = _courseId, Title = "Protected",
            Content = "Mine", OrderIndex = 0, CreatedAt = DateTime.UtcNow
        };
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.DeleteLessonAsync(lesson.Id, Guid.NewGuid()));
    }
}
