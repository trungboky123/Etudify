using back_end.Dto.Response;

namespace back_end.Service.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllCategories();
}