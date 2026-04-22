using back_end.Dto.Request;
using back_end.Dto.Response;

namespace back_end.Service.Interfaces;

public interface ICourseService
{
    Task<List<CourseResponse>> GetLastestCourse();
    Task<int> GetTotalCourses();
    Task<List<CourseResponse>> GetAllCourses(string? keyword, int? categoryId, string? instructorId, bool? status);
    Task ToggleStatus(int courseId);
    Task<PageableResponse<CourseResponse>> GetPublicCourses(int page, int pageSize, string? keyword, int? categoryId, string? sortByPrice);
    Task AddCourse(AddCourseRequest request);
    Task<CourseResponse> GetCourseById(int courseId);
}