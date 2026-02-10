namespace Lms.Api.Entities;

public class Lesson
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }

    public Course Course { get; set; } = null!;
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
