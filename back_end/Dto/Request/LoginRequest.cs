namespace back_end.Dto.Request;

public class LoginRequest
{
    public string UsernameOrEmail { get; set; }
    public string Password { get; set; }
    public bool IsRememberMe { get; set; }
}