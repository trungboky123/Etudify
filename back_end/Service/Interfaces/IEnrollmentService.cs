using back_end.Dto.Response;

namespace back_end.Service.Interfaces;

public interface IEnrollmentService
{
    Task Enroll(string userId, int itemId);
    Task<bool> HasEnrolled(string userId, int itemId);
    Task<List<EnrollmentResponse>> GetByUserId(string userId, string? keyword, int? categoryId, string? sortBy, string? sortDir);
}