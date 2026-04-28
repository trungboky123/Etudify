using back_end.Database;
using back_end.Entity;
using back_end.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repository.Classes;

public class WishlistRepository : IWishlistRepository
{
    private readonly AppDbContext _context;
    
    public  WishlistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string userId, int itemId)
    {
        return await _context.Wishlists
            .AnyAsync(w => w.UserId == userId && w.ItemId == itemId);
    }

    public async Task DeleteAsync(string userId, int itemId)
    {
        var wishlist = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ItemId == itemId);
        if (wishlist == null) return;
        _context.Wishlists.Remove(wishlist);
        await _context.SaveChangesAsync();
    }

    public IQueryable<Wishlist> GetByUserIdAsync(string userId)
    {
        return _context.Wishlists
            .Include(w => w.Item)
            .Where(w => w.UserId == userId);
    }
}