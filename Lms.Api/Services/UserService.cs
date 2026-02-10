using Lms.Api.Data;
using Lms.Api.DTOs.Admin;
using Lms.Api.DTOs.Common;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedResult<StudentListDto>> GetAllUsersAsync(string? search, int page, int pageSize)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(s) || u.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new StudentListDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                EnrolledCourses = u.Enrollments.Count
            })
            .ToListAsync();

        return new PaginatedResult<StudentListDto>
        {
            Items = users,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<StudentDetailDto> GetUserByIdAsync(Guid id)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Enrollments)
                .ThenInclude(e => e.Course)
                    .ThenInclude(c => c.Lessons)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        var lessonProgressMap = await _db.LessonProgresses
            .AsNoTracking()
            .Where(lp => lp.UserId == id && lp.IsCompleted)
            .Select(lp => lp.LessonId)
            .ToListAsync();

        var enrollments = user.Enrollments.Select(e =>
        {
            var totalLessons = e.Course.Lessons.Count;
            var completedLessons = e.Course.Lessons.Count(l => lessonProgressMap.Contains(l.Id));
            return new StudentEnrollmentDto
            {
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                ProgressPercent = totalLessons > 0 ? (int)((double)completedLessons / totalLessons * 100) : 0
            };
        }).ToList();

        return new StudentDetailDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            Enrollments = enrollments
        };
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            throw new KeyNotFoundException("User not found.");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task<StudentListDto> UpdateRoleAsync(Guid id, UpdateRoleDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            throw new KeyNotFoundException("User not found.");

        user.Role = dto.Role;
        await _db.SaveChangesAsync();

        return new StudentListDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            EnrolledCourses = await _db.Enrollments.CountAsync(e => e.UserId == id)
        };
    }
}
