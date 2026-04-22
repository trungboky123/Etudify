using back_end.Dto.Response;

namespace back_end.Service.Interfaces;

public interface ILessonService
{
    Task<List<LessonResponse>> GetLessonsByCourseId(int courseId);
}