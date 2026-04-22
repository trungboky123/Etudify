namespace back_end.Dto.Request;

public class VerifyTokenRequest
{
    public string UserId { get; set; }
    public string Token { get; set; }
}