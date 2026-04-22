using back_end.Entity;

namespace back_end.Repository.Interfaces;

public interface ILessonRepository
{
    Task<List<Lesson>> GetLessonsByCourseId(int courseId);
}