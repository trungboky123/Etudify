namespace back_end.Entity;

public class Enrollment
{
    public int Id { get; set; }
    public string UserId  { get; set; }
    public User User { get; set; }
    public int ItemId { get; set; }
    public Course Item { get; set; }
    public DateTime EnrolledAt { get; set; }
    public bool Status { get; set; }
}