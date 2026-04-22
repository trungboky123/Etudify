namespace back_end.Entity;

public class Lesson
{
    public int Id { get; set; }
    public string Name { get; set; } 
    public int CourseId { get; set; }
    public Course Course { get; set; }
    public string Description { get; set; }
    public string VideoUrl { get; set; }
    public string PdfUrl { get; set; }
    public int Order { get; set; }
    public bool IsPreview { get; set; }
    public bool Status  { get; set; }
}