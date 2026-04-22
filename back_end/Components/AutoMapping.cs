using AutoMapper;
using back_end.Dto.Response;
using back_end.Entity;

namespace back_end.Components;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        CreateMap<Category, CategoryResponse>();
        CreateMap<Course, CourseResponse>();
        CreateMap<User, UserResponse>();
        CreateMap<Lesson, LessonResponse>();
    }
}