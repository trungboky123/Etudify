using back_end.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controller;

[ApiController]
[Route("/courses")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
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
}