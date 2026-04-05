namespace back_end.Repository.Interfaces;

public interface IPaymentRepository
{
    Task<decimal> SumAmount();
}