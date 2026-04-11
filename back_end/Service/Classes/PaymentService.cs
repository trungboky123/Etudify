using back_end.Dto.Response;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;

namespace back_end.Service.Classes;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<decimal> TotalRevenue()
    {
        return await _paymentRepository.SumAmount();
    }

    public async Task<List<MonthlyRevenueResponse>> GetMonthlyRevenue()
    {
        var data = await _paymentRepository.GetMonthlyRevenue();
        var revenueDict = data.ToDictionary(x => x.Month, x => x.Revenue);

        return Enumerable.Range(1, 12)
            .Select(month => new MonthlyRevenueResponse
            {
                Month = month,
                Revenue = revenueDict.GetValueOrDefault(month, 0)
            })
            .ToList();
    }

    public async Task<List<CourseSoldResponse>> GetTopSoldCourses()
    {
        return await _paymentRepository.GetTopSoldCourses();
    }
}