using back_end.Dto.Response;

namespace back_end.Service.Interfaces;

public interface ICourseService
{
    Task<List<CourseResponse>> GetLastestCourse();
}