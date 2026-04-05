namespace back_end.Dto.Response;

public class UserResponse
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string AvatarUrl { get; set; }
    public bool Status { get; set; }
}