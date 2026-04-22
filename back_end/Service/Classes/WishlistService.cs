using back_end.Database;
using back_end.Dto.Response;
using back_end.Entity;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;
using Microsoft.Extensions.Localization;

namespace back_end.Service.Classes;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly AppDbContext _context;

    public WishlistService(IWishlistRepository wishlistRepository,  IStringLocalizer<Messages> localizer, AppDbContext context)
    {
        _wishlistRepository = wishlistRepository;
        _localizer = localizer;
        _context = context;
    }

    public async Task<bool> ExistsByUserIdAndItemId(string userId, int itemId)
    {
        return await _wishlistRepository.ExistsAsync(userId, itemId);
    }

    public async Task Add(string userId, int itemId)
    {
        if (await ExistsByUserIdAndItemId(userId, itemId))
        {
            throw new ApiExceptionResponse(_localizer["WishlistExisted"]);
        }

        Wishlist wishlist = new Wishlist
        {
            UserId = userId,
            ItemId = itemId,
            AddedAt = DateTime.UtcNow
        };
        
        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(string userId, int itemId)
    {
        await _wishlistRepository.DeleteAsync(userId, itemId);
    }
}