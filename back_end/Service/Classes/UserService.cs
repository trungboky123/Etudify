using System.Text;
using AutoMapper;
using back_end.Components;
using back_end.Dto.Request;
using back_end.Dto.Response;
using back_end.Service.Interfaces;
using back_end.Entity;
using back_end.Repository.Interfaces;
using back_end.Validator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;

namespace back_end.Service.Classes;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole>  _roleManager;
    private readonly RegisterValidator _registerValidator;
    private readonly UpdateUserValidator _updateUserValidator;
    private readonly AddAccountValidator _addAccountValidator;
    private readonly CloudinaryService _cloudinaryService;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly BackgroundTaskQueue _taskQueue;
    private readonly MailSender _mailSender;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private static readonly string DefaultAvatarUrl = "https://i.pinimg.com/736x/21/91/6e/21916e491ef0d796398f5724c313bbe7.jpg";

    public UserService(UserManager<User> userManager, RegisterValidator registerValidator, UpdateUserValidator updateUserValidator, CloudinaryService cloudinaryService, IStringLocalizer<Messages> localizer, BackgroundTaskQueue taskQueue, MailSender mailSender, IUserRepository userRepository, IMapper mapper, AddAccountValidator addAccountValidator, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _registerValidator = registerValidator;
        _updateUserValidator = updateUserValidator;
        _addAccountValidator = addAccountValidator;
        _cloudinaryService = cloudinaryService;
        _localizer = localizer;
        _taskQueue = taskQueue;
        _mailSender = mailSender;
        _userRepository = userRepository;
        _mapper = mapper;
        _roleManager = roleManager;
    }
    
    public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
    {
        var validate = await _registerValidator.ValidateAsync(request);
        if (!validate.IsValid)
        {
            var error = validate.Errors.First();
            return IdentityResult.Failed(new IdentityError
            {
                Description = error.ErrorMessage
            });
        }
        
        var user = new User
        {
            UserName = request.Username,
            FullName = request.FullName,
            Email = request.Email,
            AvatarUrl = "https://i.pinimg.com/736x/21/91/6e/21916e491ef0d796398f5724c313bbe7.jpg",
            Status = true,
            EmailConfirmed = false
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return result;
        }

        var role = await _userManager.AddToRoleAsync(user, "Student");
        if (!role.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return role;
        }

        await SendVerificationEmail(user);

        return IdentityResult.Success;
    }

    public async Task CodeChangeEmail(User user, string email)
    {
        var userId = user.Id;
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, email);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = $"https://lackadaisical-estrual-spencer.ngrok-free.dev/api/users/verify-new-email?&userId={userId}&email={email}&token={encoded}";
        
        _taskQueue.QueueBackgroundWorkItem(async () =>
        {
            await _mailSender.SendCodeEmailAsync(email, user.UserName, link);
        });
    }

    public async Task UpdateMe(string? userId, UpdateUserRequest request)
    {
        var validate = _updateUserValidator.Validate(request);
        if (!validate.IsValid)
        {
            var error = validate.Errors.First();
            throw new ApiExceptionResponse(error.ErrorMessage);
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return;
        
        bool updated = false;

        if (request.RemoveAvatar)
        {
            user.AvatarUrl = "https://i.pinimg.com/736x/21/91/6e/21916e491ef0d796398f5724c313bbe7.jpg";
            updated = true;
        }

        if (request.FullName != null)
        {
            if (string.IsNullOrWhiteSpace(request.FullName)) throw new ApiExceptionResponse(_localizer["FullNameRequired"]);
            user.FullName = request.FullName;
            updated = true;
        }
        
        if (request.UserName != null)
        {
            if (string.IsNullOrWhiteSpace(request.UserName)) throw new ApiExceptionResponse(_localizer["UsernameRequired"]);
            var duplicate = await _userManager.FindByNameAsync(request.UserName);
            if (duplicate != null)
            {
                throw new ApiExceptionResponse(_localizer["UsernameExisted"]);
            }
            user.UserName = request.UserName;
            updated = true;
        }

        if (request.NewPassword != null)
        {
            if (string.IsNullOrEmpty(request.CurrentPassword)) throw new ApiExceptionResponse(_localizer["CurrentPasswordRequired"]);
            if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword)) throw new ApiExceptionResponse(_localizer["CurrentPasswordIncorrect"]);

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                throw new ApiExceptionResponse(error.Description);
            }

            updated = true;
        }

        if (request.Avatar != null && request.Avatar.Length > 0)
        {
            string avatarUrl = await _cloudinaryService.UploadUserAvatar(request.Avatar, userId);
            user.AvatarUrl = avatarUrl;
            updated = true;
        }

        if (!updated) throw new ApiExceptionResponse(_localizer["NoFields"]);
        await _userManager.UpdateAsync(user);
    }

    public async Task<int> GetTotalUsers()
    {
        return await _userRepository.TotalUsers();
    }

    public async Task<List<UserResponse>> GetRecentUsers()
    {
        var users = await _userRepository.GetRecentUsers();
        return _mapper.Map<List<UserResponse>>(users);
    }

    public async Task<List<UserResponse>> GetAllUsers(int? roleId, string? keyword, bool? status)
    {
        var users = await  _userRepository.GetAllUsers(roleId, keyword, status);
        var responses = new List<UserResponse>();

        foreach (var user in users)
        {
            var response = _mapper.Map<UserResponse>(user);
            var roles = await _userManager.GetRolesAsync(user);
            response.Role = roles.FirstOrDefault();
            responses.Add(response);
        }

        return responses;
    }

    public async Task ToggleStatus(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ApiExceptionResponse(_localizer["UserNotFound"]);
        }
        
        user.Status = !user.Status;
        await _userManager.UpdateAsync(user);
    }

    public async Task CreateAccount(AddAccountRequest request)
    {
        var validate = await _addAccountValidator.ValidateAsync(request);
        if (!validate.IsValid)
        {
            var error = validate.Errors.First();
            throw new ApiExceptionResponse(error.ErrorMessage);
        }

        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            UserName = request.Username,
            Status = request.Status,
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            throw new ApiExceptionResponse(createResult.Errors.First().Description);
        }
        var role = await _roleManager.FindByIdAsync(request.RoleId);
        var roleAdd = await _userManager.AddToRoleAsync(user, role.Name);
        if (!roleAdd.Succeeded)
        {
            await  _userManager.DeleteAsync(user);
            return;
        }

        if (request.Avatar != null)
        {
            string avatarUrl = await _cloudinaryService.UploadUserAvatar(request.Avatar, user.Id);
            user.AvatarUrl = avatarUrl;
        }
        else
        {
            user.AvatarUrl = DefaultAvatarUrl;
        }
        
        await _userManager.UpdateAsync(user);
        await SendResetPassword(user);
    }

    public async Task SendVerificationEmail(User user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = $"https://lackadaisical-estrual-spencer.ngrok-free.dev/api/auth/verify-email?&userId={user.Id}&email={user.Email}&token={encoded}";
        _taskQueue.QueueBackgroundWorkItem(async () =>
        {
            await _mailSender.SendCodeEmailAsync(user.Email, user.UserName, link);
        });
    }

    public async Task SendResetPassword(User user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = $"https://lackadaisical-estrual-spencer.ngrok-free.dev/api/auth/reset-password?&userId={user.Id}&token={encoded}";
        _taskQueue.QueueBackgroundWorkItem(async () =>
        {
            await _mailSender.SendResetPasswordAsync(user.Email, user.UserName, link);
        });
    }

    public async Task<List<UserResponse>> GetAllInstructors()
    {
        var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
        return _mapper.Map<List<UserResponse>>(instructors);
    }
}