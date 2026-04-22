namespace back_end.Dto.Response;

public class LessonResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsPreview { get; set; }
    public string Slug { get; set; }
    public bool Status { get; set; }
}