using back_end.Database;
using back_end.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repository.Classes;

public class PaymentRepsitory : IPaymentRepository
{
    private readonly AppDbContext _context;
    
    public PaymentRepsitory(AppDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> SumAmount()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);
        return await _context.Payments
            .Where(p => p.Status == "Paid" &&
                        p.CreatedAt >= startOfMonth &&
                        p.CreatedAt < startOfNextMonth)
            .SumAsync(p => p.Amount);
    }
}