# LMS Backend API

A Learning Management System backend built with **ASP.NET Core 9**, **EF Core Code First**, and **PostgreSQL**.

## Tech Stack

- .NET 9 Web API
- Entity Framework Core 9 (Code First)
- PostgreSQL (Production – Render) / Docker (Local)
- JWT Authentication
- BCrypt password hashing
- Swagger / Swashbuckle
- xUnit + EF Core InMemory for unit testing

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
│   ├── Common/
│   ├── Course/
│   ├── Enrollment/
│   ├── Lesson/
│   └── LessonProgress/
├── Entities/             # EF Core entities
├── Middleware/            # Global exception handling
├── Migrations/
├── Services/
│   └── Interfaces/
└── Program.cs

Lms.Tests/
└── Services/             # Unit tests (xUnit + InMemory DB)
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
- PostgreSQL (or Docker)

### Run PostgreSQL with Docker

```bash
docker run -e "POSTGRES_PASSWORD=YourPassword123" \
  -p 5432:5432 --name lms-pg \
  -d postgres:16
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
       "DefaultConnection": "Host=localhost;Port=5432;Database=LmsDb;Username=postgres;Password=YOUR_PASSWORD"
     },
     "Jwt": {
       "Key": "YourSecretKeyAtLeast32BytesLong!",
       "Issuer": "LmsApi",
       "Audience": "LmsApiUsers",
       "ExpiryInHours": "3"
     }
   }
   ```

3. Run (auto-migrates on startup)
   ```bash
   cd Lms.Api
   dotnet run
   ```

4. Open Swagger UI at `http://localhost:5038/swagger`

### Running Tests

```bash
# Run all tests
make test

# Or without Make
dotnet test --verbosity normal

# Run tests for a specific service
make test-auth
make test-course
make test-enrollment
```

## Deploy to Render

1. Create **PostgreSQL** on Render (Free tier)
2. Create **Web Service** → connect GitHub repo → Runtime: **Docker**
3. Set environment variables:

   | Key | Value |
   |-----|-------|
   | `DATABASE_URL` | *(auto from linked Render PostgreSQL)* |
   | `Jwt__Key` | Your secret key (32+ bytes) |
   | `Jwt__Issuer` | `LmsApi` |
   | `Jwt__Audience` | `LmsApiUsers` |
   | `Jwt__ExpiryInHours` | `3` |

### Seed Data

The app auto-migrates and seeds on first run:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@lms.com | Admin@123 |
| Student | studenta@lms.com | Student@123 |
| Student | studentb@lms.com | Student@123 |
| Student | studentc@lms.com | Student@123 |

3 courses with lessons and sample enrollments are also seeded.
