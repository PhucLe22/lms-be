using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Auth;

public class RegisterDto
{
    [Required, MaxLength(200)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; init; } = string.Empty;
}
