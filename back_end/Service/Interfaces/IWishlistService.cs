using back_end.Dto.Response;

namespace back_end.Service.Interfaces;

public interface IWishlistService
{
    Task<bool> ExistsByUserIdAndItemId(string userId, int itemId);
    Task Add(string userId, int itemId);
    Task Delete(string userId, int itemId);
    Task<List<WishlistResponse>> GetByUserId(string userId, string? keyword, int? categoryId, string? sortBy, string? sortDir);
}