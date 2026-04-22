using back_end.Database;
using back_end.Entity;
using back_end.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repository.Classes;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public void CreateToken(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
    }

    public async Task<RefreshToken?> FindByTokenAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task<List<RefreshToken>> FindByUserIdAsync(string userId)
    {
        return await _context.RefreshTokens.Where(x => x.UserId == userId)
            .ToListAsync();
    }
}