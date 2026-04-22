namespace back_end.Repository.Interfaces;

public interface IEnrollmentRepository
{
    Task<bool> ExistsByItemIdAndUserIdAsync(string userId, int itemId);
}