namespace back_end.Dto.Response;

public class EnrollmentResponse
{
    public int ItemId { get; set; }
    public string Name { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string Slug { get; set; }
    public DateTime EnrolledAt { get; set; }
    public List<CategoryResponse>  Categories { get; set; }

    public EnrollmentResponse(int itemId, string name, string? thumbnailUrl, string slug, DateTime enrolledAt, List<CategoryResponse> categories)
    {
        ItemId = itemId;
        Name = name;
        ThumbnailUrl = thumbnailUrl;
        Slug = slug;
        EnrolledAt = enrolledAt;
        Categories = categories;
    }
}