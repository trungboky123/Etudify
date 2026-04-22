using AutoMapper;
using back_end.Dto.Response;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;
using Slugify;

namespace back_end.Service.Classes;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IMapper _mapper;
    private readonly SlugHelper _slugHelper;

    public LessonService(ILessonRepository lessonRepository, IMapper mapper, SlugHelper slugHelper)
    {
        _lessonRepository = lessonRepository;
        _mapper = mapper;
        _slugHelper = slugHelper;
    }

    public async Task<List<LessonResponse>> GetLessonsByCourseId(int courseId)
    {
        var lessons = await _lessonRepository.GetLessonsByCourseId(courseId);
        return lessons.Select(lesson =>
        {
            var response = _mapper.Map<LessonResponse>(lesson);
            response.Slug = _slugHelper.GenerateSlug(response.Name);
            return response;
        }).ToList();
    }
}