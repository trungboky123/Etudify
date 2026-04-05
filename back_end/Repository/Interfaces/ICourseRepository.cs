
namespace back_end.Repository.Interfaces;

public interface ICourseRepository
{
    Task<List<Entity.Course>> GetLatestCoursesAsync();
}