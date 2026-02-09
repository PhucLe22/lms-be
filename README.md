# LMS Backend API

A Learning Management System backend built with **ASP.NET Core 9**, **EF Core Code First**, and **SQL Server**.

## Tech Stack

- .NET 9 Web API
- Entity Framework Core 9 (Code First)
- SQL Server
- JWT Authentication
- BCrypt password hashing
- Swagger / Swashbuckle

## Architecture

```
Controller → Service → DbContext (no Repository pattern)
```

```
Lms.Api/
├── Controllers/          # API endpoints
├── Data/                 # DbContext & seed data
├── DTOs/                 # Request/response models
│   ├── Auth/
│   ├── Course/
│   ├── Enrollment/
│   └── Lesson/
├── Entities/             # EF Core entities
├── Migrations/
├── Services/
│   └── Interfaces/
└── Program.cs
```

## API Endpoints

### Auth (Public)
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login & get JWT token |
| GET | `/api/auth/me` | Get current user info (JWT) |

### Courses (Public read, Admin write)
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/courses` | Public |
| GET | `/api/courses/{id}` | Public |
| POST | `/api/courses` | Admin |
| PUT | `/api/courses/{id}` | Admin |
| DELETE | `/api/courses/{id}` | Admin |

### Lessons (Public read, Admin write)
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/courses/{courseId}/lessons` | Public |
| POST | `/api/courses/{courseId}/lessons` | Admin |
| PUT | `/api/lessons/{id}` | Admin |
| DELETE | `/api/lessons/{id}` | Admin |

### Enrollments (Student only)
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/enrollments/{courseId}` | Enroll in a course |
| GET | `/api/enrollments/my-courses` | Get my enrollments |
| DELETE | `/api/enrollments/{courseId}` | Unenroll from a course |

### Health
| Method | Route |
|--------|-------|
| GET | `/health` |

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (or Docker)

### Run SQL Server with Docker

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123" \
  -p 1433:1433 --name lms-sql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

### Setup

1. Clone the repo
   ```bash
   git clone https://github.com/PhucLe22/lms-be.git
   cd lms-be
   ```

2. Create `Lms.Api/appsettings.Development.json` with your secrets
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=LmsDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
     },
     "Jwt": {
       "Key": "YourSecretKeyAtLeast32BytesLong!",
       "Issuer": "LmsApi",
       "Audience": "LmsApiUsers",
       "ExpiryInHours": "3"
     }
   }
   ```

3. Apply migrations & run
   ```bash
   cd Lms.Api
   dotnet ef database update
   dotnet run
   ```

4. Open Swagger UI at `http://localhost:5038/swagger`

### Seed Data

The app seeds automatically on first run:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@lms.com | Admin@123 |
| Student | studenta@lms.com | Student@123 |
| Student | studentb@lms.com | Student@123 |
| Student | studentc@lms.com | Student@123 |

3 courses with lessons and sample enrollments are also seeded.
