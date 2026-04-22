using back_end.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controller;

[ApiController]
[Route("/api/lessons")]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    [AllowAnonymous]
    [HttpGet("{courseId}")]
    public async Task<IActionResult> GetLessonsByCourseId([FromRoute] int courseId)
    {
        var results =  await _lessonService.GetLessonsByCourseId(courseId);
        return Ok(results);
    }
}