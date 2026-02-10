using System.Text.Json;
using Lms.Api.Data;
using Lms.Api.DTOs.Common;
using Lms.Api.DTOs.Course;
using Lms.Api.DTOs.Lesson;
using Lms.Api.Entities;
using Lms.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Lms.Api.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CourseService> _logger;

    private const string CacheKeyAllPrefix = "courses:all";
    private const string CacheKeySinglePrefix = "courses:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public CourseService(AppDbContext db, IDistributedCache cache, ILogger<CourseService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PaginatedResult<CourseDto>> GetAllCoursesAsync(string? search, string? level, int page, int pageSize)
    {
        // Only cache when no filters applied (default page)
        var cacheKey = string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(level)
            ? $"{CacheKeyAllPrefix}:p{page}:s{pageSize}"
            : null;

        if (cacheKey is not null)
        {
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached is not null)
            {
                _logger.LogInformation("Cache HIT for {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<PaginatedResult<CourseDto>>(cached)!;
            }
        }

        var query = _db.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(term) ||
                c.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(c => c.Level == level);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Level = c.Level,
                CreatorName = c.Creator.FullName,
                CreatedAt = c.CreatedAt,
                LessonCount = c.Lessons.Count,
                EnrollmentCount = c.Enrollments.Count
            })
            .ToListAsync();

        var result = new PaginatedResult<CourseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        // Cache the result
        if (cacheKey is not null)
        {
            _logger.LogInformation("Cache MISS → storing {CacheKey}", cacheKey);
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), options);
        }

        return result;
    }

    public async Task<CourseDetailDto> GetCourseByIdAsync(Guid courseId)
    {
        var cacheKey = $"{CacheKeySinglePrefix}{courseId}";

        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            _logger.LogInformation("Cache HIT for {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<CourseDetailDto>(cached)!;
        }

        var course = await _db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseDetailDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Level = c.Level,
                CreatorName = c.Creator.FullName,
                CreatedAt = c.CreatedAt,
                Lessons = c.Lessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => new LessonDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Content = l.Content,
                        VideoUrl = l.VideoUrl,
                        DocumentUrl = l.DocumentUrl,
                        OrderIndex = l.OrderIndex,
                        CreatedAt = l.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (course is null)
            throw new KeyNotFoundException($"Course {courseId} not found.");

        _logger.LogInformation("Cache MISS → storing {CacheKey}", cacheKey);
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(course), options);

        return course;
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto, Guid creatorId)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Level = dto.Level,
            CreatedBy = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        await InvalidateCoursesCacheAsync();

        var creatorName = await _db.Users
            .Where(u => u.Id == creatorId)
            .Select(u => u.FullName)
            .FirstAsync();

        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Level = course.Level,
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
        course.Level = dto.Level;
        await _db.SaveChangesAsync();

        await InvalidateCoursesCacheAsync(courseId);

        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Level = course.Level,
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

        await InvalidateCoursesCacheAsync(courseId);
    }

    /// <summary>
    /// Invalidate all course list caches and optionally a specific course cache.
    /// </summary>
    private async Task InvalidateCoursesCacheAsync(Guid? courseId = null)
    {
        _logger.LogInformation("Invalidating courses cache");

        // Invalidate common list pages (first 5 pages with common sizes)
        foreach (var pageSize in new[] { 10, 20, 50 })
        {
            for (var page = 1; page <= 5; page++)
            {
                await _cache.RemoveAsync($"{CacheKeyAllPrefix}:p{page}:s{pageSize}");
            }
        }

        // Invalidate specific course
        if (courseId.HasValue)
        {
            await _cache.RemoveAsync($"{CacheKeySinglePrefix}{courseId.Value}");
        }
    }
}
