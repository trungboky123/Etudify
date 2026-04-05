using System.Security.Claims;
using System.Text;
using back_end.Components;
using back_end.Dto.Request;
using back_end.Dto.Response;
using back_end.Entity;
using back_end.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;

namespace back_end.Controller;

[ApiController]
[Route("/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly UserManager<User> _userManager;

    public UserController(IUserService userService, IStringLocalizer<Messages> localizer, UserManager<User> userManager)
    {
        _userService = userService;
        _localizer = localizer;
        _userManager = userManager;
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        var user = await _userManager.FindByIdAsync(userId);
        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        var userResponse = new UserResponse
        {
            Id = userId,
            Username = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Role = role,
            AvatarUrl = user.AvatarUrl
        };

        return Ok(userResponse);
    }

    [Authorize]
    [HttpPatch("me")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMe([FromForm] UpdateUserRequest request, [FromForm] IFormFile? avatar)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _userService.UpdateMe(userId, request, avatar);

        return Ok(new { message = _localizer["UpdateSuccess"] });
    }

    [Authorize]
    [HttpPost("check-email")]
    public async Task<IActionResult> CheckCurrentEmail([FromQuery] string email)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        var user = await _userManager.FindByIdAsync(userId);
        var currentEmail = user.Email;
        if (!string.Equals(currentEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = _localizer["CurrentEmailIncorrect"]});
        }

        return Ok(new { message = _localizer["EmailVerified"] });
    }

    [Authorize]
    [HttpPost("code-change-email")]
    public async Task<IActionResult> SendCodeChangeEmail([FromQuery] string email)
    {
        var duplicate = await _userManager.FindByEmailAsync(email);
        if (duplicate != null)
        {
            return BadRequest(new { message = _localizer["EmailExisted"] });
        }

        var user = await _userManager.GetUserAsync(User);
        await _userService.CodeChangeEmail(user, email);
        return Ok();
    }

    [Authorize]
    [HttpGet("verify-new-email")]
    public async Task<IActionResult> VerifyNewEmail([FromQuery] string userId, [FromQuery] string email, [FromQuery] string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();
        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ChangeEmailAsync(user, email, decoded);
        if (!result.Succeeded)
        {
            return Redirect($"http://localhost:5173/verify-email?token={token}&status=false");
        }
        return Redirect($"http://localhost:5173/verify-email?token={token}&status=true");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("total")]
    public async Task<IActionResult> GetTotalUsers()
    {
        var totalUsers = await _userService.GetTotalUsers();
        return Ok(new { totalUsers = totalUsers });
    }
}