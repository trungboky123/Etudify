using back_end.Entity;

namespace back_end.Repository.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategories();
    Task<List<Category>> GetByIds(List<int> ids);
}