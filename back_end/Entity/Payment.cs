namespace back_end.Entity;

public class Payment
{
    public long Id { get; set; }
    public long OrderCode {get; set;}
    public string UserId { get; set; }
    public User User { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; }
    public string Status { get; set; }
    public  DateTime CreatedAt { get; set; }
    public DateTime PaidAt { get; set; }
}