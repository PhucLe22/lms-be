using Lms.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
            return; // Already seeded

        // ── 1. Users ────────────────────────────────────────
        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Admin LMS",
            Email = "admin@lms.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        var student1 = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Nguyen Van A",
            Email = "studenta@lms.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        };

        var student2 = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Tran Thi B",
            Email = "studentb@lms.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        };

        var student3 = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Le Van C",
            Email = "studentc@lms.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(admin, student1, student2, student3);

        // ── 2. Courses ──────────────────────────────────────
        var course1 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "C# Fundamentals",
            Description = "Learn C# from scratch: variables, OOP, LINQ, async/await.",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course2 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "ASP.NET Core Web API",
            Description = "Build RESTful APIs with ASP.NET Core, EF Core, JWT Authentication.",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course3 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "SQL Server & EF Core",
            Description = "Database design, T-SQL, Entity Framework Core Code First, migrations.",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        db.Courses.AddRange(course1, course2, course3);

        // ── 3. Lessons ──────────────────────────────────────
        // Course 1: C# Fundamentals (5 lessons)
        db.Lessons.AddRange(
            new Lesson { Id = Guid.NewGuid(), CourseId = course1.Id, Title = "Introduction to C#", Content = "Overview of C#, .NET ecosystem, setting up development environment.", OrderIndex = 1, CreatedAt = DateTime.UtcNow },
            new Lesson { Id = Guid.NewGuid(), CourseId = course1.Id, Title = "Variables & Data Types", Content = "Primitive types, string, var, const, type casting, nullable types.", OrderIndex = 2, CreatedAt = DateTime.UtcNow },
            new Lesson { Id = Guid.NewGuid(), CourseId = course1.Id, Title = "OOP in C#", Content = "Classes, objects, inheritance, polymorphism, interfaces, abstract classes.", OrderIndex = 3, CreatedAt = DateTime.UtcNow },
            new Lesson { Id = Guid.NewGuid(), CourseId = course1.Id, Title = "LINQ", Content = "Query syntax, method syntax, Where, Select, OrderBy, GroupBy, Join.", OrderIndex = 4, CreatedAt = DateTime.UtcNow },
            new Lesson { Id = Guid.NewGuid(), CourseId = course1.Id, Title = "Async/Await", Content = "Task, async, await, Task.WhenAll, cancellation tokens.", OrderIndex = 5, CreatedAt = DateTime.UtcNow }
        );

        // Course 2: ASP.NET Core Web API (4 lessons)
        db.Lessons.AddRange(
            new Lesson { Id = Guid.NewGuid(), CourseId = course2.Id, Title = "Project Setup", Content = "Create Web API project, folder structure, Program.cs configuration.", OrderIndex = 1, CreatedAt = DateTime.UtcNow },
            new Lesson { Id = Guid.NewGuid(), CourseId = course2.Id, Title = "Controllers & Routing", Content = "ApiController, Route attributes, HTTP methods, model binding.", OrderIndex = 2, CreatedAt = DateTime.UtcNow },
            new Lesson { Id = Guid.NewGuid(), CourseId = course2.Id, Title = "Dependency Injection & Services", Content = "DI container, AddScoped/Transient/Singleton, service layer pattern.", OrderIndex = 3, CreatedAt = DateTime.UtcNow },
            new Lesson { Id = Guid.NewGuid(), CourseId = course2.Id, Title = "JWT Authentication", Content = "JWT tokens, claims, role-based authorization, [Authorize] attribute.", OrderIndex = 4, CreatedAt = DateTime.UtcNow }
        );

        // Course 3: SQL Server & EF Core (3 lessons)
        db.Lessons.AddRange(
            new Lesson { Id = Guid.NewGuid(), CourseId = course3.Id, Title = "Database Design", Content = "Tables, primary keys, foreign keys, normalization, ERD diagrams.", OrderIndex = 1, CreatedAt = DateTime.UtcNow },
            new Lesson { Id = Guid.NewGuid(), CourseId = course3.Id, Title = "EF Core Code First", Content = "DbContext, entities, Fluent API, migrations, seeding data.", OrderIndex = 2, CreatedAt = DateTime.UtcNow },
            new Lesson { Id = Guid.NewGuid(), CourseId = course3.Id, Title = "Querying with EF Core", Content = "LINQ to Entities, Include, AsNoTracking, raw SQL, performance tips.", OrderIndex = 3, CreatedAt = DateTime.UtcNow }
        );

        // ── 4. Enrollments ──────────────────────────────────
        db.Enrollments.AddRange(
            new Enrollment { Id = Guid.NewGuid(), UserId = student1.Id, CourseId = course1.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student1.Id, CourseId = course2.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student2.Id, CourseId = course1.Id, EnrolledAt = DateTime.UtcNow, Status = "Completed" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student3.Id, CourseId = course3.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" }
        );

        await db.SaveChangesAsync();
    }
}
