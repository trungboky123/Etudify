using System.Text;
using back_end.Components;
using back_end.Dto.Request;
using back_end.Dto.Response;
using back_end.Entity;
using back_end.Service.Interfaces;
using back_end.Validator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;

namespace back_end.Controller;

[ApiController]
[Route("/api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly JwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly ResetPasswordValidator _validator;

    public AuthController(IUserService userService, UserManager<User> userManager, JwtService jwtService,
        IRefreshTokenService refreshTokenService, IConfiguration configuration, IStringLocalizer<Messages> localizer, ResetPasswordValidator validator)
    {
        _userService = userService;
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
        _localizer = localizer;
        _validator = validator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request);
        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return BadRequest(new { message = error.Description });
        }

        return Ok(new { message = _localizer["RegisterSuccess"] });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string email,
        [FromQuery] string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return BadRequest();
        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decoded);
        if (!result.Succeeded)
        {
            return Redirect($"https://lackadaisical-estrual-spencer.ngrok-free.dev/verify-email?token={token}&status=false");
        }

        return Redirect($"https://lackadaisical-estrual-spencer.ngrok-free.dev/verify-email?token={token}&status=true");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.UsernameOrEmail) ??
                   await _userManager.FindByNameAsync(request.UsernameOrEmail);
        if (user == null)
        {
            return Unauthorized(new { message = _localizer["UsernameEmailNotFound"] });
        }

        if (!user.EmailConfirmed)
        {
            await _userService.SendVerificationEmail(user);
            return Unauthorized(new { message = _localizer["EmailNotVerified"] });
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            return Unauthorized(new { message = _localizer["PasswordIncorrect"] });
        }

        var oldTokens = await _refreshTokenService.FindByUserIdAsync(user.Id);
        if (oldTokens.Any())
        {
            foreach (var token in oldTokens)
            {
                await _refreshTokenService.RevokeRefreshToken(token);
            }
        }

        var accessToken = await _jwtService.GenerateAccessToken(user);
        var refreshToken = await _refreshTokenService.CreateRefreshToken(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault();

        var refreshExpiry = request.IsRememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(1);

        Response.Cookies.Append("accessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Path = "/",
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]))
        });

        Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Path = "/",
            Expires = refreshExpiry
        });

        return Ok(new
        {
            message = _localizer["LoginSuccess"],
            role = roleName
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        var token = await _refreshTokenService.FindByTokenAsync(refreshToken);

        if (token == null || token.IsRevoked) return Unauthorized();

        if (token.ExpiredDate < DateTime.UtcNow)
        {
            token.IsRevoked = true;
            return Unauthorized(new { message = _localizer["RefreshTokenInvalid"] });
        }

        var user = await _userManager.FindByIdAsync(token.UserId);
        if (user == null)
        {
            return Unauthorized();
        }

        var newAccessToken = await _jwtService.GenerateAccessToken(user);

        Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Path = "/",
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]))
        });

        return Ok(new { accessToken = newAccessToken });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(token))
        {
            var refreshToken = await _refreshTokenService.FindByTokenAsync(token);
            if (refreshToken != null)
            {
                await _refreshTokenService.RevokeRefreshToken(refreshToken);
            }
        }

        Response.Cookies.Append("refreshToken", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(-1)
        });

        Response.Cookies.Append("accessToken", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(-1)
        });

        return Ok();
    }
    
    [HttpPost("verify-password-token")]
    public async Task<IActionResult> VerifyPasswordToken([FromBody] VerifyTokenRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return BadRequest(new { message = _localizer["UserNotFound"] });
        }

        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        var isValid = await _userManager.VerifyUserTokenAsync(
            user,
            _userManager.Options.Tokens.PasswordResetTokenProvider,
            "ResetPassword",
            decoded
        );

        if (!isValid)
        {
            return BadRequest();
        }

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return BadRequest(new { message = _localizer["UserNotFound"] });
        }
        
        var isValid = _validator.Validate(request);
        if (!isValid.IsValid)
        {
            return BadRequest(new { message = isValid.Errors.FirstOrDefault().ErrorMessage });
        }
        
        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        var result = await _userManager.ResetPasswordAsync(user, decoded, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ApiExceptionResponse(_localizer["PasswordInvalid"]);
        }
        
        return Ok(new { message = _localizer["ChangePasswordSuccess"] });
    }

}