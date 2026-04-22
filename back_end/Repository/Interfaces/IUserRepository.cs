using back_end.Entity;

namespace back_end.Repository.Interfaces;

public interface IUserRepository
{
    Task<int> TotalUsers();
    Task<List<User>> GetRecentUsers();
    Task<List<User>> GetAllUsers(int? roleId, string? keyword, bool? status);
    Task<bool> UsernameExists(string username);
    Task<bool> EmailExists(string email);
}