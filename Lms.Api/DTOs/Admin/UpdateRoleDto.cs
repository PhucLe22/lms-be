using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Admin;

public class UpdateRoleDto
{
    [Required]
    [RegularExpression("^(Admin|Student)$", ErrorMessage = "Role must be 'Admin' or 'Student'.")]
    public string Role { get; init; } = string.Empty;
}
