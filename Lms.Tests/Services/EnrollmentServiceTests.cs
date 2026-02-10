using Lms.Api.Data;
using Lms.Api.Entities;
using Lms.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Lms.Tests.Services;

public class EnrollmentServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly EnrollmentService _sut;
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();

    public EnrollmentServiceTests()
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
            Id = _courseId,
            Title = "Test Course",
            Description = "Test",
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        });
        _db.SaveChanges();

        _sut = new EnrollmentService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task EnrollAsync_Success_ReturnsEnrollmentDto()
    {
        var result = await _sut.EnrollAsync(_studentId, _courseId);

        Assert.Equal(_courseId, result.CourseId);
        Assert.Equal("Test Course", result.CourseTitle);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task EnrollAsync_AlreadyEnrolled_ThrowsInvalidOperation()
    {
        _db.Enrollments.Add(new Enrollment
        {
            Id = Guid.NewGuid(),
            UserId = _studentId,
            CourseId = _courseId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Active"
        });
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.EnrollAsync(_studentId, _courseId));
    }

    [Fact]
    public async Task EnrollAsync_CourseNotFound_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.EnrollAsync(_studentId, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetMyEnrollmentsAsync_ReturnsUserEnrollments()
    {
        _db.Enrollments.Add(new Enrollment
        {
            Id = Guid.NewGuid(),
            UserId = _studentId,
            CourseId = _courseId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Active"
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetMyEnrollmentsAsync(_studentId);

        Assert.Single(result);
        Assert.Equal("Test Course", result[0].CourseTitle);
    }

    [Fact]
    public async Task GetMyEnrollmentsAsync_NoEnrollments_ReturnsEmpty()
    {
        var result = await _sut.GetMyEnrollmentsAsync(_studentId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task UnenrollAsync_Success_RemovesEnrollment()
    {
        _db.Enrollments.Add(new Enrollment
        {
            Id = Guid.NewGuid(),
            UserId = _studentId,
            CourseId = _courseId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Active"
        });
        await _db.SaveChangesAsync();

        await _sut.UnenrollAsync(_studentId, _courseId);

        Assert.False(await _db.Enrollments.AnyAsync());
    }

    [Fact]
    public async Task UnenrollAsync_NotEnrolled_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UnenrollAsync(_studentId, _courseId));
    }
}
