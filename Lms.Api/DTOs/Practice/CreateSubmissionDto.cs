using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Practice;

public class CreateSubmissionDto
{
    [Required]
    [RegularExpression("^(Text|GitUrl)$", ErrorMessage = "SubmissionType must be 'Text' or 'GitUrl'.")]
    public string SubmissionType { get; init; } = "Text";

    [Required, MaxLength(5000)]
    public string Content { get; init; } = string.Empty;
}
