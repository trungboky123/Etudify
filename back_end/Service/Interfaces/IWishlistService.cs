namespace back_end.Service.Interfaces;

public interface IWishlistService
{
    Task<bool> ExistsByUserIdAndItemId(string userId, int itemId);
    Task Add(string userId, int itemId);
    Task Delete(string userId, int itemId);
}