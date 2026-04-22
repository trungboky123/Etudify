namespace back_end.Entity;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal? ListedPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string InstructorId { get; set; }
    public User Instructor { get; set; }
    public int Duration { get; set; }
    public string? Description { get; set; }
    public bool Status { get; set; }
    public ICollection<Category> Categories { get; set; }= new List<Category>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Lesson>  Lessons { get; set; } = new List<Lesson>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public  ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}