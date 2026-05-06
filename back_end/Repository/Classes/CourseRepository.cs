using back_end.Database;
using Microsoft.EntityFrameworkCore;
using back_end.Entity;
using back_end.Repository.Interfaces;

namespace back_end.Repository.Classes;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Course>> GetLatestCoursesAsync()
    {
        return await _context.Courses
            .Where(c => c.Status == true)
            .OrderByDescending(x => x.Id)
            .Include(x => x.Categories)
            .Take(8)
            .ToListAsync();
    }

    public async Task<int> GetTotalCoursesAsync()
    {
        return await _context.Courses.CountAsync();
    }

    public IQueryable<Course> GetAllCourses()
    {
        return _context.Courses
            .Include(c => c.Categories)
            .Include(c => c.Instructor);
    }

    public IQueryable<Course> GetPublicCourses()
    {
        return _context.Courses
            .Include(c => c.Categories)
            .Include(c => c.Instructor)
            .Where(c => c.Status == true);
    }

    public async Task<Course?> GetCourseByIdAsync(int courseId)
    {
        return await _context.Courses   
            .Include(c => c.Categories)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }

    public async Task<List<Course>> GetAllByIdsAsync(List<int> courseIds)
    {
        return await _context.Courses
            .Include(c => c.Categories)
            .Include(c => c.Instructor)
            .Where(c => courseIds.Contains(c.Id))
            .ToListAsync();
    }
}