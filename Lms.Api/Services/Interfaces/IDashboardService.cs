using Lms.Api.DTOs.Dashboard;

namespace Lms.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetStudentDashboardAsync(Guid userId);
}
