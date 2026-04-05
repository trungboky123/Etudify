using back_end.Entity;

namespace back_end.Repository.Interfaces;

public interface IUserRepository
{
    Task<int> TotalUsers();
}