namespace back_end.Service.Interfaces;

public interface IEnrollmentService
{
    Task Enroll(string userId, int itemId);
    Task<bool> HasEnrolled(string userId, int itemId);
}