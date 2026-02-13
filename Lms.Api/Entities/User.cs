namespace Lms.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Role { get; set; } = "Student";
    public DateTime CreatedAt { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // OAuth
    public string? GoogleId { get; set; }
    public string? AvatarUrl { get; set; }

    public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
    public ICollection<PracticeSubmission> PracticeSubmissions { get; set; } = new List<PracticeSubmission>();
}
