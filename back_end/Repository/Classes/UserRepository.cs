using back_end.Database;
using back_end.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repository.Classes;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> TotalUsers()
    {
        return await _context.Users.CountAsync();
    }
}