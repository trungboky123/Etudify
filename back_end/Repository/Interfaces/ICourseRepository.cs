using back_end.Entity;

namespace back_end.Repository.Interfaces;

public interface ICourseRepository
{
    Task<List<Course>> GetLatestCoursesAsync();
    Task<int> GetTotalCoursesAsync();
    IQueryable<Course> GetAllCourses(); 
    IQueryable<Course> GetPublicCourses();
    Task<Course?> GetCourseByIdAsync(int courseId);
}