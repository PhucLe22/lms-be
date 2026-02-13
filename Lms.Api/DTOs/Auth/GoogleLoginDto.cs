using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Auth;

public class GoogleLoginDto
{
    /// <summary>
    /// Google ID Token (preferred) or Access Token
    /// </summary>
    [Required]
    public string IdToken { get; init; } = string.Empty;
}
