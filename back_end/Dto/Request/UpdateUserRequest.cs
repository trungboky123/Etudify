namespace back_end.Dto.Request;

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? AvatarUrl { get; set; }
    public IFormFile? Avatar { get; set; }
    public int? RoleId { get; set; }
    public bool? Status { get; set; }
    public bool RemoveAvatar { get; set; }
}