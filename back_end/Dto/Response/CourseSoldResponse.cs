namespace back_end.Dto.Response;

public class CourseSoldResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}