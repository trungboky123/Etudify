using back_end.Dto.Response;

namespace back_end.Service.Interfaces;

public interface IPaymentService
{
    Task<decimal> TotalRevenue();
    Task<List<MonthlyRevenueResponse>> GetMonthlyRevenue();
    Task<List<CourseSoldResponse>> GetTopSoldCourses();
}