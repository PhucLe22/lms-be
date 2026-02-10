namespace Lms.Api.Entities;

public class Quiz
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }

    public Lesson Lesson { get; set; } = null!;
}
