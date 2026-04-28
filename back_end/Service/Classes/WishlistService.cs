using back_end.Database;
using back_end.Dto.Response;
using back_end.Entity;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Slugify;

namespace back_end.Service.Classes;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly AppDbContext _context;
    private readonly SlugHelper _slugHelper;

    public WishlistService(IWishlistRepository wishlistRepository,  IStringLocalizer<Messages> localizer, AppDbContext context, SlugHelper slugHelper)
    {
        _wishlistRepository = wishlistRepository;
        _localizer = localizer;
        _context = context;
        _slugHelper = slugHelper;
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

    public async Task<List<WishlistResponse>> GetByUserId(string userId, string? keyword, int? categoryId, string? sortBy, string? sortDir)
    {
        var query = _wishlistRepository.GetByUserIdAsync(userId);
        
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(w => w.Item.Name.ToLower().Contains(keyword.ToLower()));
        }
        
        if (categoryId.HasValue)
        {
            query = query.Where(w => w.Item.Categories.Any(cat => cat.Id == categoryId));
        }
        
        if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortDir))
        {
            query = (sortBy, sortDir) switch
            {
                ("date", "asc") => query.OrderBy(w => w.AddedAt),
                ("date", "desc") => query.OrderByDescending(w => w.AddedAt),
                ("price", "asc") => query.OrderBy(w => w.Item.SalePrice ?? w.Item.ListedPrice),
                ("price", "desc") => query.OrderByDescending(w => w.Item.SalePrice ?? w.Item.ListedPrice)
            };
        }

        var wishlists = await query.ToListAsync();
        
        return wishlists.Select(w => new WishlistResponse(
            w.ItemId,
            w.Item.Name,
            _slugHelper.GenerateSlug(w.Item.Name),
            w.Item.ListedPrice,
            w.Item.SalePrice,
            w.Item.ThumbnailUrl,
            w.AddedAt
        )).ToList();
    }
}