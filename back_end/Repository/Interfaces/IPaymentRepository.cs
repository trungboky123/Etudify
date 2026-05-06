using back_end.Dto.Response;
using back_end.Entity;

namespace back_end.Repository.Interfaces;

public interface IPaymentRepository
{
    Task<decimal> SumAmount();
    Task<List<MonthlyRevenueResponse>> GetMonthlyRevenue();
    Task<List<CourseSoldResponse>> GetTopSoldCourses();
    Task<Payment?> GetByOrderCodeAsync(long orderCode);
    Task<List<Payment>> GetByUserIdAsync(string userId);
}