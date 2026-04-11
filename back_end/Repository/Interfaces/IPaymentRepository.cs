using back_end.Dto.Response;

namespace back_end.Repository.Interfaces;

public interface IPaymentRepository
{
    Task<decimal> SumAmount();
    Task<List<MonthlyRevenueResponse>> GetMonthlyRevenue();
    Task<List<CourseSoldResponse>> GetTopSoldCourses();
}