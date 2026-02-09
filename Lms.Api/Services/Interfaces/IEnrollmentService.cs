using Lms.Api.DTOs.Enrollment;

namespace Lms.Api.Services.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollAsync(Guid userId, Guid courseId);
    Task<List<EnrollmentDto>> GetMyEnrollmentsAsync(Guid userId);
    Task UnenrollAsync(Guid userId, Guid courseId);
}
