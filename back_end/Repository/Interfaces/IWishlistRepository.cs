namespace back_end.Repository.Interfaces;

public interface IWishlistRepository
{
    Task<bool> ExistsAsync(string userId, int itemId);
    Task DeleteAsync(string userId, int itemId);
}