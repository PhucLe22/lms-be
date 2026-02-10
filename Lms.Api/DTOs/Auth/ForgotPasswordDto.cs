using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}
