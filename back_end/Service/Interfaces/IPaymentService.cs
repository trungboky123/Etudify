namespace back_end.Service.Interfaces;

public interface IPaymentService
{
    Task<decimal> TotalRevenue();
}