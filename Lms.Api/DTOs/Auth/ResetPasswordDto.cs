using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Auth;

public class ResetPasswordDto
{
    [Required]
    public string Token { get; init; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; init; } = string.Empty;
}
