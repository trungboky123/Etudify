using back_end.Dto.Request;
using back_end.Dto.Response;

namespace back_end.Service.Interfaces;

public interface IPaymentService
{
    Task<decimal> TotalRevenue();
    Task<List<MonthlyRevenueResponse>> GetMonthlyRevenue();
    Task<List<CourseSoldResponse>> GetTopSoldCourses();
    Task<Dictionary<string, object>> CreatePayment(string userId, PaymentRequest request);
    Task<string> GetPaymentStatus(long orderCode);
}