using Lms.Api.Data;
using Lms.Api.DTOs.Enrollment;
using Lms.Api.Entities;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _db;

    public EnrollmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EnrollmentDto> EnrollAsync(Guid userId, Guid courseId)
    {
        var courseExists = await _db.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
            throw new KeyNotFoundException($"Course {courseId} not found.");

        var alreadyEnrolled = await _db.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (alreadyEnrolled)
            throw new InvalidOperationException("Already enrolled in this course.");

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Active"
        };

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();

        var courseTitle = await _db.Courses
            .Where(c => c.Id == courseId)
            .Select(c => c.Title)
            .FirstAsync();

        return new EnrollmentDto
        {
            Id = enrollment.Id,
            CourseId = enrollment.CourseId,
            CourseTitle = courseTitle,
            EnrolledAt = enrollment.EnrolledAt,
            Status = enrollment.Status
        };
    }

    public async Task<List<EnrollmentDto>> GetMyEnrollmentsAsync(Guid userId)
    {
        return await _db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Select(e => new EnrollmentDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status
            })
            .ToListAsync();
    }

    public async Task UnenrollAsync(Guid userId, Guid courseId)
    {
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (enrollment is null)
            throw new KeyNotFoundException("Enrollment not found.");

        _db.Enrollments.Remove(enrollment);
        await _db.SaveChangesAsync();
    }
}
