using back_end.Database;
using back_end.Entity;

namespace back_end.Repository.Interfaces;

public interface IRefreshTokenRepository
{
    void CreateToken(RefreshToken token);
    Task<RefreshToken?> FindByTokenAsync(string token);
    Task<List<RefreshToken>> FindByUserIdAsync(string userId);
}