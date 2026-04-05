namespace back_end.Entity;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public DateTime ExpiredDate { get; set; }
    public bool IsRevoked { get; set; }
}