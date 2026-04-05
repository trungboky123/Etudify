using back_end.Dto.Request;
using back_end.Entity;
using Microsoft.AspNetCore.Identity;

namespace back_end.Service.Interfaces;

public interface IUserService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
    Task<IdentityResult> RegisterCheck(RegisterRequest request);
    Task UpdateMe(string? userId, UpdateUserRequest request, IFormFile? avatar);
    Task CodeChangeEmail(User user, string email);
    Task<int> GetTotalUsers();
}