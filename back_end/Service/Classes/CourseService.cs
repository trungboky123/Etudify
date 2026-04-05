using AutoMapper;
using back_end.Dto.Response;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;
using Slugify;

namespace back_end.Service.Classes;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;
    private readonly SlugHelper _slugHelper;

    public CourseService(ICourseRepository courseRepository, IMapper mapper, SlugHelper slugHelper)
    {
        _courseRepository = courseRepository;
        _mapper = mapper;
        _slugHelper = slugHelper;
    }

    public async Task<List<CourseResponse>> GetLastestCourse()
    {
        var courses = await _courseRepository.GetLatestCoursesAsync();
        return courses.Select(course =>
        {
            var response = _mapper.Map<CourseResponse>(course);
            response.Slug = _slugHelper.GenerateSlug(response.Name);
            return response;
        }).ToList();
    }
}