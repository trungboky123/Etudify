using back_end.Entity;

namespace back_end.Service.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateRefreshToken(User user);
    Task<RefreshToken?> FindByTokenAsync(string token);
    Task RevokeRefreshToken(RefreshToken token);
    Task<List<RefreshToken>> FindByUserIdAsync(string userId);
}