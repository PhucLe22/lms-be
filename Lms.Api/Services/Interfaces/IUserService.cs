using Lms.Api.DTOs.Admin;
using Lms.Api.DTOs.Common;

namespace Lms.Api.Services.Interfaces;

public interface IUserService
{
    Task<PaginatedResult<StudentListDto>> GetAllUsersAsync(string? search, int page, int pageSize);
    Task<StudentDetailDto> GetUserByIdAsync(Guid id);
    Task DeleteUserAsync(Guid id);
    Task<StudentListDto> UpdateRoleAsync(Guid id, UpdateRoleDto dto);
}
