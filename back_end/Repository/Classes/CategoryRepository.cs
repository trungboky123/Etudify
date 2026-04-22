using back_end.Database;
using back_end.Entity;
using back_end.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repository.Classes;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
    
    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllCategories()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<List<Category>> GetByIds(List<int> ids)
    {
        return await _context.Categories
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();
    }
}