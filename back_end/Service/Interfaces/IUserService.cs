using back_end.Dto.Request;
using back_end.Dto.Response;
using back_end.Entity;
using Microsoft.AspNetCore.Identity;

namespace back_end.Service.Interfaces;

public interface IUserService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
    Task UpdateMe(string? userId, UpdateUserRequest request);
    Task CodeChangeEmail(User user, string email);
    Task<int> GetTotalUsers();
    Task<List<UserResponse>> GetRecentUsers();
    Task<List<UserResponse>> GetAllUsers(int? roleId, string? keyword, bool? status);
    Task ToggleStatus(string userId);
    Task CreateAccount(AddAccountRequest request);
    Task SendVerificationEmail(User user);
    Task SendResetPassword(User user);
    Task<List<UserResponse>> GetAllInstructors();
    Task<UserResponse> GetUserById(string userId);
    Task UpdateUser(string? userId, UpdateUserRequest request);
}