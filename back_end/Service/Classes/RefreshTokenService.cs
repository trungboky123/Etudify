using System.Security.Cryptography;
using back_end.Database;
using back_end.Entity;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;

namespace back_end.Service.Classes;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RefreshTokenService(AppDbContext context, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
    {
        _context = context;
        _configuration = configuration;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<RefreshToken> CreateRefreshToken(User user)
    {
        var days = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]);
        var token = new RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiredDate = DateTime.UtcNow.AddDays(days),
            IsRevoked = false
        };

        _refreshTokenRepository.CreateToken(token);
        await _context.SaveChangesAsync();

        return token;
    }

    public async Task<RefreshToken?> FindByTokenAsync(string token)
    {
        return await _refreshTokenRepository.FindByTokenAsync(token);
    }

    public async Task RevokeRefreshToken(RefreshToken token)
    {
        token.IsRevoked = true;
        await _context.SaveChangesAsync();
    }
}