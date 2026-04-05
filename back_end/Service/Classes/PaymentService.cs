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
}