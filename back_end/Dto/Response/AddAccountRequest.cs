namespace back_end.Dto.Response;

public class AddAccountRequest
{
    public string FullName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public IFormFile? Avatar { get; set; }
    public string? AvatarUrl { get; set; }
    public string RoleId { get; set; }
    public bool Status { get; set; }
}