namespace back_end.Dto.Request;

public class AddCourseRequest
{
    public string Name { get; set; } 
    public decimal? ListedPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public List<int> CategoryIds { get; set; }
    public IFormFile? Thumbnail { get; set; }
    public string InstructorId { get; set; }
    public int Duration { get; set; }
    public string? Description { get; set; }
    public bool Status { get; set; }
}