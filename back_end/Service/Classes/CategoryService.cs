using AutoMapper;
using back_end.Dto.Response;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;

namespace back_end.Service.Classes;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<List<CategoryResponse>> GetAllCategories()
    {
       var categories = await _categoryRepository.GetAllCategories();
       return categories.Select(category => _mapper.Map<CategoryResponse>(category)).ToList();
    }
}