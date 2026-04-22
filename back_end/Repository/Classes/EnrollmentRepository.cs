using back_end.Database;
using back_end.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repository.Classes;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _context;
    
    public EnrollmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByItemIdAndUserIdAsync(string userId, int itemId)
    {
        return await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.ItemId == itemId);
    }
}