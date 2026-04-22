using AutoMapper;
using back_end.Components;
using back_end.Database;
using back_end.Dto.Request;
using back_end.Dto.Response;
using back_end.Entity;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;
using back_end.Validator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Slugify;

namespace back_end.Service.Classes;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly SlugHelper _slugHelper;
    private readonly AppDbContext _context;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly AddCourseValidator _validator;
    private readonly HtmlService _htmlService;
    private readonly CloudinaryService _cloudinaryService;
    private readonly IHubContext<CourseHub> _hubContext;

    public CourseService(ICourseRepository courseRepository, IMapper mapper, SlugHelper slugHelper, AppDbContext context,  IStringLocalizer<Messages> localizer,  AddCourseValidator validator, ICategoryRepository categoryRepository,  HtmlService htmlService, CloudinaryService cloudinaryService, IHubContext<CourseHub> hubContext)
    {
        _courseRepository = courseRepository;
        _mapper = mapper;
        _slugHelper = slugHelper;
        _context = context;
        _localizer = localizer;
        _validator = validator;
        _categoryRepository = categoryRepository;
        _htmlService = htmlService;
        _cloudinaryService = cloudinaryService;
        _hubContext = hubContext;
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

    public async Task<int> GetTotalCourses()
    {
        return await _courseRepository.GetTotalCoursesAsync();
    }

    public async Task<List<CourseResponse>> GetAllCourses(string? keyword, int? categoryId, string? instructorId, bool? status)
    {
        var query = _courseRepository.GetAllCourses();
        if (status != null)
        {
            query = query.Where(c => c.Status == status);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(c => c.Name.Contains(keyword));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(c => c.Categories.Any(cat => cat.Id == categoryId.Value));
        }

        if (!string.IsNullOrEmpty(instructorId))
        {
            query = query.Where(c => c.InstructorId == instructorId);
        }

        var courses = await query.ToListAsync();
        return _mapper.Map<List<CourseResponse>>(courses);
    }

    public async Task ToggleStatus(int courseId)
    {
        var course = await _courseRepository.GetCourseByIdAsync(courseId);
        if (course == null)
        {
            throw new ApiExceptionResponse(_localizer["CourseNotFound"]);
        }
        
        course.Status = !course.Status;
        await _context.SaveChangesAsync();
    }

    public async Task<PageableResponse<CourseResponse>> GetPublicCourses(int page, int pageSize, string? keyword, int? categoryId,
        string? sortByPrice)
    {
        var query = _courseRepository.GetPublicCourses();
        if (categoryId.HasValue)
        {
            query = query.Where(c => c.Categories.Any(cat => cat.Id == categoryId));
        }
        
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(c => c.Name.ToLower().Contains(keyword.ToLower()));
        }
        
        if (sortByPrice == "asc")
        {
            query = query.OrderBy(c => c.SalePrice ?? c.ListedPrice);
        }
        else if (sortByPrice == "desc")
        {
            query = query.OrderByDescending(c => c.SalePrice ?? c.ListedPrice);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var courses = await query.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var results = courses.Select(course =>
        {
            var res = _mapper.Map<CourseResponse>(course);
            res.Slug = _slugHelper.GenerateSlug(res.Name);
            return res;
        }).ToList();

        return new PageableResponse<CourseResponse>
        {
            Items = results,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task AddCourse(AddCourseRequest request)
    {
        var validate =  _validator.Validate(request);
        if (!validate.IsValid)
        {
            throw new ApiExceptionResponse(validate.Errors.FirstOrDefault().ErrorMessage);
        }

        var categories = await _categoryRepository.GetByIds(request.CategoryIds);
        var course = new Course
        {
            Name = request.Name,
            ListedPrice = request.ListedPrice,
            SalePrice = request.SalePrice,
            InstructorId = request.InstructorId,
            Categories = categories,
            Duration = request.Duration,
            Description = _htmlService.Sanitize(request.Description),
            Status = request.Status,
        };
        
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        if (request.Thumbnail != null)
        {
            string thumbnailUrl = await _cloudinaryService.UploadCourseThumbnail(request.Thumbnail, course.Id);
            course.ThumbnailUrl = thumbnailUrl;
        }
        
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("CourseAdded");
    }

    public async Task<CourseResponse> GetCourseById(int courseId)
    {
        var course = await _courseRepository.GetCourseByIdAsync(courseId);
        if (course == null)
        {
            throw new ApiExceptionResponse(_localizer["CourseNotFound"]);
        }
        
        var response = _mapper.Map<CourseResponse>(course);
        response.Slug = _slugHelper.GenerateSlug(response.Name);
        return response;
    }
}