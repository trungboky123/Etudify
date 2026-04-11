using back_end.Database;
using back_end.Entity;
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
        return await _context.Users
            .Where(u => u.EmailConfirmed)
            .CountAsync();
    }

    public async Task<List<User>> GetRecentUsers()
    {
        return await _context.Users
            .OrderByDescending(u => u.Id)
            .Take(3)
            .ToListAsync();
    }

    public async Task<List<User>> GetAllUsers(int? roleId, string? keyword, bool? status)
    {
        var query = _context.Users.AsQueryable();
        query = query.Where(u => u.EmailConfirmed);

        if (status != null)
        {
            query = query.Where(u => u.Status == status);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(u => u.UserName.Contains(keyword) || u.Email.Contains(keyword));
        }

        if (roleId.HasValue)
        {
            query = from u in query
                join ur in _context.UserRoles on u.Id equals ur.UserId
                where ur.RoleId == roleId.Value.ToString()
                    select u;
        }

        var users = await query.ToListAsync();
        return users;
    }
}