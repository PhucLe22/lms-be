# ── Database ──
db-up:
	docker compose up -d

db-down:
	docker compose down

# ── App ──
run:
	dotnet run --project Lms.Api

build:
	dotnet build

# ── Tests ──
test:
	dotnet test Lms.Tests --verbosity normal

test-auth:
	dotnet test Lms.Tests --filter "FullyQualifiedName~AuthServiceTests" --verbosity normal

test-course:
	dotnet test Lms.Tests --filter "FullyQualifiedName~CourseServiceTests" --verbosity normal

test-enrollment:
	dotnet test Lms.Tests --filter "FullyQualifiedName~EnrollmentServiceTests" --verbosity normal

# ── Migrations ──
migrate:
	dotnet ef database update --project Lms.Api

migration-add:
	@read -p "Migration name: " name; \
	dotnet ef migrations add $$name --project Lms.Api
