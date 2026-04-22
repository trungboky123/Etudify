using back_end.Dto.Request;
using back_end.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace back_end.Controller;

[ApiController]
[Route("/api/courses")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IStringLocalizer<Messages> _localizer;

    public CourseController(ICourseService courseService, IStringLocalizer<Messages> localizer)
    {
        _courseService = courseService;
        _localizer = localizer;
    }

    [HttpGet("latest")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLastestCourses()
    {
        var courses = await _courseService.GetLastestCourse();
        return Ok(courses);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("total")]
    public async Task<IActionResult> GetTotalCourses()
    {
        var count =  await _courseService.GetTotalCourses();
        return Ok(new { totalCourses = count });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllCourses([FromQuery] string? keyword, [FromQuery] int? categoryId, [FromQuery] string? instructorId, [FromQuery] bool? status)
    {
        var courses = await _courseService.GetAllCourses(keyword, categoryId, instructorId, status);
        return Ok(courses);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("status/{courseId}")]
    public async Task<IActionResult> ToggleStatus([FromRoute] int courseId)
    {
        await _courseService.ToggleStatus(courseId);
        return Ok();
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublicCourses(
        [FromQuery] int? categoryId,
        [FromQuery] string? keyword,
        [FromQuery] bool? status,
        [FromQuery] string? sortByPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12
    )
    {
        var result = await _courseService.GetPublicCourses(page, pageSize, keyword, categoryId, sortByPrice);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("add")]
    public async Task<IActionResult> AddCourse([FromForm] AddCourseRequest request)
    {
        await _courseService.AddCourse(request);
        return Ok(new { message = _localizer["CourseCreated"] });
    }

    [AllowAnonymous]
    [HttpGet("{courseId}")]
    public async Task<IActionResult> GetCourseById([FromRoute] int courseId)
    {
        var result = await _courseService.GetCourseById(courseId);
        return Ok(result);
    }
}