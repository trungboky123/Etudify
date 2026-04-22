namespace back_end.Dto.Response;

public class CourseResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal ListedPrice { get; set; }
    public List<CategoryResponse> Categories { get; set; }
    public decimal? SalePrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public UserResponse Instructor { get; set; }
    public int Duration { get; set; }
    public string? Description { get; set; }
    public string Slug { get; set; }
    public bool Status { get; set; }
}