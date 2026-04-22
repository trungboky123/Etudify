using back_end.Database;
using back_end.Entity;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;

namespace back_end.Service.Classes;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _context;
    private readonly IEnrollmentRepository _enrollmentRepository;
    
    public EnrollmentService(AppDbContext context,  IEnrollmentRepository enrollmentRepository)
    {
        _context = context;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task Enroll(string userId, int itemId)
    {
        Enrollment enrollment = new Enrollment
        {
            UserId = userId,
            ItemId = itemId,
            EnrolledAt = DateTime.UtcNow,
            Status = true
        };
        
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasEnrolled(string userId, int itemId)
    {
        return await _enrollmentRepository.ExistsByItemIdAndUserIdAsync(userId, itemId);
    }
}