using Lms.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lms.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizResult> QuizResults => Set<QuizResult>();
    public DbSet<PracticeTask> PracticeTasks => Set<PracticeTask>();
    public DbSet<PracticeSubmission> PracticeSubmissions => Set<PracticeSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.PasswordHash).IsRequired(false);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

            entity.Property(u => u.PasswordResetToken).HasMaxLength(128);
            entity.Property(u => u.PasswordResetTokenExpiry);

            entity.Property(u => u.GoogleId).HasMaxLength(128);
            entity.HasIndex(u => u.GoogleId).IsUnique().HasFilter("\"GoogleId\" IS NOT NULL");
            entity.Property(u => u.AvatarUrl).HasMaxLength(500);
        });

        // ── Course ──
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Title).IsRequired().HasMaxLength(300);
            entity.Property(c => c.Description).HasMaxLength(2000);
            entity.Property(c => c.Level).IsRequired().HasMaxLength(20).HasDefaultValue("Beginner");
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

            entity.HasOne(c => c.Creator)
                  .WithMany(u => u.CreatedCourses)
                  .HasForeignKey(c => c.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Lesson ──
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Title).IsRequired().HasMaxLength(300);
            entity.Property(l => l.Content).IsRequired();
            entity.Property(l => l.VideoUrl).HasMaxLength(500);
            entity.Property(l => l.DocumentUrl).HasMaxLength(500);
            entity.Property(l => l.OrderIndex).IsRequired();
            entity.Property(l => l.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

            entity.HasOne(l => l.Course)
                  .WithMany(c => c.Lessons)
                  .HasForeignKey(l => l.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(l => new { l.CourseId, l.OrderIndex }).IsUnique();
        });

        // ── Enrollment ──
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("now() at time zone 'utc'");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Enrollments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
        });

        // ── LessonProgress ──
        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(lp => lp.Id);

            entity.Property(lp => lp.VideoWatchPercent).IsRequired().HasDefaultValue(0);
            entity.Property(lp => lp.IsCompleted).IsRequired();

            entity.HasOne(lp => lp.User)
                  .WithMany(u => u.LessonProgresses)
                  .HasForeignKey(lp => lp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lp => lp.Lesson)
                  .WithMany(l => l.LessonProgresses)
                  .HasForeignKey(lp => lp.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(lp => new { lp.UserId, lp.LessonId }).IsUnique();
        });

        // ── Quiz ──
        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(q => q.Id);

            entity.Property(q => q.Question).IsRequired().HasMaxLength(1000);
            entity.Property(q => q.OptionA).IsRequired().HasMaxLength(500);
            entity.Property(q => q.OptionB).IsRequired().HasMaxLength(500);
            entity.Property(q => q.OptionC).IsRequired().HasMaxLength(500);
            entity.Property(q => q.OptionD).IsRequired().HasMaxLength(500);
            entity.Property(q => q.CorrectAnswer).IsRequired().HasMaxLength(1);
            entity.Property(q => q.OrderIndex).IsRequired();
            entity.Property(q => q.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

            entity.HasOne(q => q.Lesson)
                  .WithMany(l => l.Quizzes)
                  .HasForeignKey(q => q.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── QuizResult ──
        modelBuilder.Entity<QuizResult>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.TotalQuestions).IsRequired();
            entity.Property(r => r.CorrectAnswers).IsRequired();
            entity.Property(r => r.Score).IsRequired();
            entity.Property(r => r.CompletedAt).HasDefaultValueSql("now() at time zone 'utc'");

            entity.HasOne(r => r.User)
                  .WithMany(u => u.QuizResults)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Lesson)
                  .WithMany()
                  .HasForeignKey(r => r.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => new { r.UserId, r.LessonId }).IsUnique();
        });

        // ── PracticeTask ──
        modelBuilder.Entity<PracticeTask>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title).IsRequired().HasMaxLength(300);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(5000);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

            entity.HasOne(t => t.Lesson)
                  .WithMany()
                  .HasForeignKey(t => t.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── PracticeSubmission ──
        modelBuilder.Entity<PracticeSubmission>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.SubmissionType).IsRequired().HasMaxLength(20);
            entity.Property(s => s.Content).IsRequired().HasMaxLength(5000);
            entity.Property(s => s.SubmittedAt).HasDefaultValueSql("now() at time zone 'utc'");

            entity.HasOne(s => s.PracticeTask)
                  .WithMany(t => t.Submissions)
                  .HasForeignKey(s => s.PracticeTaskId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.User)
                  .WithMany(u => u.PracticeSubmissions)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
