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

        // ── 2. Courses (9 courses) ─────────────────────────
        var course1 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "C# Fundamentals",
            Description = "Learn C# from scratch: variables, OOP, LINQ, async/await.",
            Level = "Beginner",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course2 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "ASP.NET Core Web API",
            Description = "Build RESTful APIs with ASP.NET Core, EF Core, JWT Authentication.",
            Level = "Intermediate",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course3 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "SQL Server & EF Core",
            Description = "Database design, T-SQL, Entity Framework Core Code First, migrations.",
            Level = "Intermediate",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course4 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "HTML, CSS & JavaScript",
            Description = "Nền tảng phát triển web: HTML5, CSS3, JavaScript ES6+, DOM manipulation.",
            Level = "Beginner",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course5 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "React.js Frontend Development",
            Description = "Xây dựng giao diện người dùng với React, Hooks, Router, State Management.",
            Level = "Intermediate",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course6 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Docker & DevOps Basics",
            Description = "Container hóa ứng dụng với Docker, Docker Compose, CI/CD pipeline cơ bản.",
            Level = "Intermediate",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course7 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Git & GitHub Mastery",
            Description = "Quản lý source code với Git, branching strategy, pull request, collaboration.",
            Level = "Beginner",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course8 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Data Structures & Algorithms",
            Description = "Cấu trúc dữ liệu và giải thuật: Array, LinkedList, Tree, Graph, Sorting, Searching.",
            Level = "Advanced",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        var course9 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Software Testing & QA",
            Description = "Unit testing, integration testing, xUnit, Moq, test-driven development (TDD).",
            Level = "Intermediate",
            CreatedBy = admin.Id,
            CreatedAt = DateTime.UtcNow
        };

        db.Courses.AddRange(course1, course2, course3, course4, course5, course6, course7, course8, course9);

        // ── 3. Lessons ──────────────────────────────────────

        // Course 1: C# Fundamentals (5 lessons)
        db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course1.Id,
                Title = "Introduction to C#",
                Content = "Overview of C#, .NET ecosystem, setting up development environment.",
                VideoUrl = "https://www.youtube.com/watch?v=GhQdlMFylQ8",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/",
                OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course1.Id,
                Title = "Variables & Data Types",
                Content = "Primitive types, string, var, const, type casting, nullable types.",
                VideoUrl = "https://www.youtube.com/watch?v=_D-HCF0yFpM",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/",
                OrderIndex = 2, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course1.Id,
                Title = "OOP in C#",
                Content = "Classes, objects, inheritance, polymorphism, interfaces, abstract classes.",
                VideoUrl = "https://www.youtube.com/watch?v=pTB0EiLXUC8",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/",
                OrderIndex = 3, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course1.Id,
                Title = "LINQ",
                Content = "Query syntax, method syntax, Where, Select, OrderBy, GroupBy, Join.",
                VideoUrl = "https://www.youtube.com/watch?v=5l2qA3Pc83M",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/csharp/linq/",
                OrderIndex = 4, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course1.Id,
                Title = "Async/Await",
                Content = "Task, async, await, Task.WhenAll, cancellation tokens.",
                VideoUrl = "https://www.youtube.com/watch?v=il9gl8MN17s",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/",
                OrderIndex = 5, CreatedAt = DateTime.UtcNow
            }
        );

        // Course 2: ASP.NET Core Web API (4 lessons)
        db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course2.Id,
                Title = "Project Setup",
                Content = "Create Web API project, folder structure, Program.cs configuration.",
                VideoUrl = "https://www.youtube.com/watch?v=AhAxLiGC7Pc",
                DocumentUrl = "https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api",
                OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course2.Id,
                Title = "Controllers & Routing",
                Content = "ApiController, Route attributes, HTTP methods, model binding.",
                VideoUrl = "https://www.youtube.com/watch?v=YVhz5_TNmkE",
                DocumentUrl = "https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing",
                OrderIndex = 2, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course2.Id,
                Title = "Dependency Injection & Services",
                Content = "DI container, AddScoped/Transient/Singleton, service layer pattern.",
                VideoUrl = "https://www.youtube.com/watch?v=Hhpq7oYcpGE",
                DocumentUrl = "https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection",
                OrderIndex = 3, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course2.Id,
                Title = "JWT Authentication",
                Content = "JWT tokens, claims, role-based authorization, [Authorize] attribute.",
                VideoUrl = "https://www.youtube.com/watch?v=v7q3pEK1EA0",
                DocumentUrl = "https://learn.microsoft.com/en-us/aspnet/core/security/authentication/",
                OrderIndex = 4, CreatedAt = DateTime.UtcNow
            }
        );

        // Course 3: SQL Server & EF Core (3 lessons)
        db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course3.Id,
                Title = "Database Design",
                Content = "Tables, primary keys, foreign keys, normalization, ERD diagrams.",
                VideoUrl = "https://www.youtube.com/watch?v=ztHopE5Wnpc",
                DocumentUrl = "https://learn.microsoft.com/en-us/sql/relational-databases/tables/",
                OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course3.Id,
                Title = "EF Core Code First",
                Content = "DbContext, entities, Fluent API, migrations, seeding data.",
                VideoUrl = "https://www.youtube.com/watch?v=SryQxUeChMc",
                DocumentUrl = "https://learn.microsoft.com/en-us/ef/core/modeling/",
                OrderIndex = 2, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course3.Id,
                Title = "Querying with EF Core",
                Content = "LINQ to Entities, Include, AsNoTracking, raw SQL, performance tips.",
                VideoUrl = "https://www.youtube.com/watch?v=Ilv0e_R_0E0",
                DocumentUrl = "https://learn.microsoft.com/en-us/ef/core/querying/",
                OrderIndex = 3, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course3.Id,
                Title = "Stored Procedures & Advanced T-SQL",
                Content = "Stored procedures, views, triggers, CTEs, window functions, query optimization.",
                VideoUrl = "https://www.youtube.com/watch?v=LnKsNq1VsPY",
                DocumentUrl = "https://learn.microsoft.com/en-us/sql/t-sql/statements/create-procedure-transact-sql",
                OrderIndex = 4, CreatedAt = DateTime.UtcNow
            }
        );

        // Course 4: HTML, CSS & JavaScript (4 lessons)
        db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course4.Id,
                Title = "HTML5 Basics",
                Content = "HTML elements, semantic tags, forms, tables, multimedia elements.",
                VideoUrl = "https://www.youtube.com/watch?v=qz0aGYrrlhU",
                DocumentUrl = "https://developer.mozilla.org/en-US/docs/Learn/HTML",
                OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course4.Id,
                Title = "CSS3 Styling & Layout",
                Content = "Selectors, Box model, Flexbox, Grid, responsive design, media queries.",
                VideoUrl = "https://www.youtube.com/watch?v=1Rs2ND1ryYc",
                DocumentUrl = "https://developer.mozilla.org/en-US/docs/Learn/CSS",
                OrderIndex = 2, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course4.Id,
                Title = "JavaScript ES6+ Fundamentals",
                Content = "Variables, functions, arrow functions, destructuring, spread operator, modules.",
                VideoUrl = "https://www.youtube.com/watch?v=hdI2bqOjy3c",
                DocumentUrl = "https://developer.mozilla.org/en-US/docs/Learn/JavaScript",
                OrderIndex = 3, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course4.Id,
                Title = "DOM Manipulation & Events",
                Content = "Document object model, selectors, event listeners, dynamic content.",
                VideoUrl = "https://www.youtube.com/watch?v=0ik6X4DJKCc",
                DocumentUrl = "https://developer.mozilla.org/en-US/docs/Web/API/Document_Object_Model",
                OrderIndex = 4, CreatedAt = DateTime.UtcNow
            }
        );

        // Course 5: React.js Frontend Development (4 lessons)
        db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course5.Id,
                Title = "React Introduction & JSX",
                Content = "Create React App, JSX syntax, components, props, rendering.",
                VideoUrl = "https://www.youtube.com/watch?v=Tn6-PIqc4UM",
                DocumentUrl = "https://react.dev/learn",
                OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course5.Id,
                Title = "React Hooks",
                Content = "useState, useEffect, useRef, useContext, useMemo, custom hooks.",
                VideoUrl = "https://www.youtube.com/watch?v=TNhaISOUy6Q",
                DocumentUrl = "https://react.dev/reference/react/hooks",
                OrderIndex = 2, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course5.Id,
                Title = "React Router & Navigation",
                Content = "React Router v6, routes, params, nested routes, navigation guards.",
                VideoUrl = "https://www.youtube.com/watch?v=Ul3y1LXxzdU",
                DocumentUrl = "https://reactrouter.com/en/main/start/tutorial",
                OrderIndex = 3, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course5.Id,
                Title = "State Management & API Integration",
                Content = "Context API, useReducer, fetching data with Axios, error handling.",
                VideoUrl = "https://www.youtube.com/watch?v=35lXWvCuM8o",
                DocumentUrl = "https://react.dev/learn/managing-state",
                OrderIndex = 4, CreatedAt = DateTime.UtcNow
            }
        );

        // Course 6: Docker & DevOps Basics (3 lessons)
        db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course6.Id,
                Title = "Docker Fundamentals",
                Content = "Containers vs VMs, Docker images, Dockerfile, building and running containers.",
                VideoUrl = "https://www.youtube.com/watch?v=pTFZFxd4hOI",
                DocumentUrl = "https://docs.docker.com/get-started/",
                OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course6.Id,
                Title = "Docker Compose",
                Content = "Multi-container apps, docker-compose.yml, networking, volumes, environment variables.",
                VideoUrl = "https://www.youtube.com/watch?v=HG6yIjZapSA",
                DocumentUrl = "https://docs.docker.com/compose/",
                OrderIndex = 2, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course6.Id,
                Title = "CI/CD Pipeline Basics",
                Content = "GitHub Actions, build-test-deploy workflow, automated testing, deployment strategies.",
                VideoUrl = "https://www.youtube.com/watch?v=R8_veQiYBjI",
                DocumentUrl = "https://docs.github.com/en/actions/quickstart",
                OrderIndex = 3, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course6.Id,
                Title = "Container Orchestration & Deployment",
                Content = "Docker Hub, container registry, Docker Swarm basics, deploying to cloud services.",
                VideoUrl = "https://www.youtube.com/watch?v=Wf2eSG3owoA",
                DocumentUrl = "https://docs.docker.com/engine/swarm/",
                OrderIndex = 4, CreatedAt = DateTime.UtcNow
            }
        );

        // Course 7: Git & GitHub Mastery (4 lessons)
        db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course7.Id,
                Title = "Git Basics",
                Content = "Init, add, commit, status, log, .gitignore, understanding staging area.",
                VideoUrl = "https://www.youtube.com/watch?v=8JJ101D3knE",
                DocumentUrl = "https://git-scm.com/doc",
                OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course7.Id,
                Title = "Branching & Merging",
                Content = "Branch, checkout, merge, rebase, resolving conflicts, branching strategies.",
                VideoUrl = "https://www.youtube.com/watch?v=e9lnsKot_SQ",
                DocumentUrl = "https://git-scm.com/book/en/v2/Git-Branching-Basic-Branching-and-Merging",
                OrderIndex = 2, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course7.Id,
                Title = "GitHub Collaboration",
                Content = "Remote repositories, push, pull, fork, pull requests, code review.",
                VideoUrl = "https://www.youtube.com/watch?v=iv8rSLsi1xo",
                DocumentUrl = "https://docs.github.com/en/pull-requests",
                OrderIndex = 3, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course7.Id,
                Title = "Advanced Git Workflows",
                Content = "Git flow, trunk-based development, cherry-pick, stash, tags, releases.",
                VideoUrl = "https://www.youtube.com/watch?v=Uszj_k0DGsg",
                DocumentUrl = "https://www.atlassian.com/git/tutorials/comparing-workflows",
                OrderIndex = 4, CreatedAt = DateTime.UtcNow
            }
        );

        // Course 8: Data Structures & Algorithms (4 lessons)
        db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course8.Id,
                Title = "Arrays & Linked Lists",
                Content = "Static vs dynamic arrays, singly/doubly linked lists, operations, complexity analysis.",
                VideoUrl = "https://www.youtube.com/watch?v=RBSGKlAvoiM",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/standard/collections/",
                OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course8.Id,
                Title = "Stacks, Queues & Hash Tables",
                Content = "LIFO, FIFO, priority queues, hashing, collision resolution, Dictionary in C#.",
                VideoUrl = "https://www.youtube.com/watch?v=wjI1WNcIntg",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/standard/collections/selecting-a-collection-class",
                OrderIndex = 2, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course8.Id,
                Title = "Trees & Graphs",
                Content = "Binary trees, BST, AVL trees, graph representations, BFS, DFS traversal.",
                VideoUrl = "https://www.youtube.com/watch?v=oSWTXtMglKE",
                DocumentUrl = "https://visualgo.net/en/bst",
                OrderIndex = 3, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course8.Id,
                Title = "Sorting & Searching Algorithms",
                Content = "Bubble, selection, insertion, merge, quick sort, binary search, Big O notation.",
                VideoUrl = "https://www.youtube.com/watch?v=kPRA0W1kECg",
                DocumentUrl = "https://visualgo.net/en/sorting",
                OrderIndex = 4, CreatedAt = DateTime.UtcNow
            }
        );

        // Course 9: Software Testing & QA (3 lessons)
        db.Lessons.AddRange(
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course9.Id,
                Title = "Unit Testing with xUnit",
                Content = "xUnit framework, Fact, Theory, Assert, test project setup, naming conventions.",
                VideoUrl = "https://www.youtube.com/watch?v=dasVbKJmvb0",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test",
                OrderIndex = 1, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course9.Id,
                Title = "Mocking & Test Doubles",
                Content = "Moq library, mock objects, stubs, fakes, testing with dependency injection.",
                VideoUrl = "https://www.youtube.com/watch?v=9ZvC6QJsdbQ",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices",
                OrderIndex = 2, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course9.Id,
                Title = "Integration Testing & TDD",
                Content = "WebApplicationFactory, in-memory database, test-driven development workflow, code coverage.",
                VideoUrl = "https://www.youtube.com/watch?v=7roqteWLw4s",
                DocumentUrl = "https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests",
                OrderIndex = 3, CreatedAt = DateTime.UtcNow
            },
            new Lesson
            {
                Id = Guid.NewGuid(), CourseId = course9.Id,
                Title = "Code Coverage & CI Testing",
                Content = "Code coverage tools, Coverlet, test reporting, integrating tests into CI/CD pipelines.",
                VideoUrl = "https://www.youtube.com/watch?v=jGG9COIFHiQ",
                DocumentUrl = "https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage",
                OrderIndex = 4, CreatedAt = DateTime.UtcNow
            }
        );

        // ── 4. Enrollments ──────────────────────────────────
        db.Enrollments.AddRange(
            new Enrollment { Id = Guid.NewGuid(), UserId = student1.Id, CourseId = course1.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student1.Id, CourseId = course2.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student1.Id, CourseId = course4.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student2.Id, CourseId = course1.Id, EnrolledAt = DateTime.UtcNow, Status = "Completed" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student2.Id, CourseId = course5.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student2.Id, CourseId = course7.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student3.Id, CourseId = course3.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student3.Id, CourseId = course6.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" },
            new Enrollment { Id = Guid.NewGuid(), UserId = student3.Id, CourseId = course8.Id, EnrolledAt = DateTime.UtcNow, Status = "Active" }
        );

        await db.SaveChangesAsync();
    }
}
