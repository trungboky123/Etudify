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
            .OrderByDescending(x => x.Id)
            .Include(x => x.Categories)
            .Take(8)
            .ToListAsync();
    }
}