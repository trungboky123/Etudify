namespace back_end.Dto.Request;

public class ResetPasswordRequest
{
    public string UserId { get; set; }
    public string NewPassword {  get; set; }
    public string Token {  get; set; }
}