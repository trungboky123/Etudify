using Microsoft.AspNetCore.Identity;

namespace back_end.Entity;

public class User : IdentityUser
{
    public string FullName { get; set; }
    public string AvatarUrl { get; set; }
    public bool Status { get; set; }
    public ICollection<Course> Courses = new List<Course>();
    public ICollection<RefreshToken> RefreshTokens = new List<RefreshToken>();
    public ICollection<Payment> Payments = new List<Payment>();
}