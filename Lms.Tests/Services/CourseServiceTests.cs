using Lms.Api.Data;
using Lms.Api.DTOs.Course;
using Lms.Api.Entities;
using Lms.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Lms.Tests.Services;

public class CourseServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CourseService _sut;
    private readonly Guid _adminId = Guid.NewGuid();

    public CourseServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        _db.Users.Add(new User
        {
            Id = _adminId,
            FullName = "Admin User",
            Email = "admin@test.com",
            PasswordHash = "hash",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        });
        _db.SaveChanges();

        _sut = new CourseService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateCourseAsync_Success_ReturnsCourseDto()
    {
        var dto = new CreateCourseDto { Title = "C# Basics", Description = "Learn C#" };

        var result = await _sut.CreateCourseAsync(dto, _adminId);

        Assert.Equal("C# Basics", result.Title);
        Assert.Equal("Learn C#", result.Description);
        Assert.Equal("Admin User", result.CreatorName);
        Assert.Equal(0, result.LessonCount);
    }

    [Fact]
    public async Task GetCourseByIdAsync_Exists_ReturnsCourseDetail()
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "EF Core",
            Description = "Learn EF",
            CreatedBy = _adminId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        var result = await _sut.GetCourseByIdAsync(course.Id);

        Assert.Equal("EF Core", result.Title);
        Assert.Equal("Admin User", result.CreatorName);
    }

    [Fact]
    public async Task GetCourseByIdAsync_NotFound_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetCourseByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateCourseAsync_OwnCourse_ReturnsUpdated()
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Old Title",
            Description = "Old Desc",
            CreatedBy = _adminId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        var dto = new UpdateCourseDto { Title = "New Title", Description = "New Desc" };

        var result = await _sut.UpdateCourseAsync(course.Id, dto, _adminId);

        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Desc", result.Description);
    }

    [Fact]
    public async Task UpdateCourseAsync_NotOwner_ThrowsUnauthorized()
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Description = "Desc",
            CreatedBy = _adminId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        var otherUserId = Guid.NewGuid();
        var dto = new UpdateCourseDto { Title = "Hack", Description = "Hacked" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.UpdateCourseAsync(course.Id, dto, otherUserId));
    }

    [Fact]
    public async Task DeleteCourseAsync_OwnCourse_RemovesFromDb()
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "To Delete",
            Description = "Bye",
            CreatedBy = _adminId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        await _sut.DeleteCourseAsync(course.Id, _adminId);

        Assert.False(await _db.Courses.AnyAsync(c => c.Id == course.Id));
    }

    [Fact]
    public async Task DeleteCourseAsync_NotOwner_ThrowsUnauthorized()
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Protected",
            Description = "Mine",
            CreatedBy = _adminId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.DeleteCourseAsync(course.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllCoursesAsync_ReturnsPaginatedResult()
    {
        for (int i = 1; i <= 3; i++)
        {
            _db.Courses.Add(new Course
            {
                Id = Guid.NewGuid(),
                Title = $"Course {i}",
                Description = $"Desc {i}",
                CreatedBy = _adminId,
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllCoursesAsync(null, page: 1, pageSize: 2);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAllCoursesAsync_WithSearch_FiltersResults()
    {
        _db.Courses.Add(new Course
        {
            Id = Guid.NewGuid(), Title = "C# Basics", Description = "Intro",
            CreatedBy = _adminId, CreatedAt = DateTime.UtcNow
        });
        _db.Courses.Add(new Course
        {
            Id = Guid.NewGuid(), Title = "SQL Guide", Description = "Database",
            CreatedBy = _adminId, CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAllCoursesAsync("c#", page: 1, pageSize: 10);

        Assert.Single(result.Items);
        Assert.Equal("C# Basics", result.Items[0].Title);
    }
}
