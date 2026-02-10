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

            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");

            entity.Property(u => u.PasswordResetToken).HasMaxLength(128);
            entity.Property(u => u.PasswordResetTokenExpiry);
        });

        // ── Course ──
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Title).IsRequired().HasMaxLength(300);
            entity.Property(c => c.Description).HasMaxLength(2000);
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
    }
}
