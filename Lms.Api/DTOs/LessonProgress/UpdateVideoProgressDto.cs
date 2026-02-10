using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.LessonProgress;

public class UpdateVideoProgressDto
{
    [Range(0, 100)]
    public int WatchPercent { get; set; }
}
