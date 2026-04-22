using back_end.Database;
using back_end.Dto.Response;
using back_end.Entity;
using back_end.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back_end.Repository.Classes;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;
    
    public PaymentRepository(AppDbContext context)
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
                        p.PaidAt >= startOfMonth &&
                        p.PaidAt < startOfNextMonth)
            .SumAsync(p => p.Amount);
    }

    public async Task<List<MonthlyRevenueResponse>> GetMonthlyRevenue()
    {
        var now = DateTime.Now;
        var startOfYear = new DateTime(now.Year, 1, 1);
        var startOfNextYear = startOfYear.AddYears(1);

        return await _context.Payments
            .Where(p => p.Status == "Paid" &&
                        p.PaidAt >= startOfYear &&
                        p.PaidAt < startOfNextYear)
            .GroupBy(p => p.PaidAt.Month)
            .Select(mrr => new MonthlyRevenueResponse
            {
                Month = mrr.Key,
                Revenue = mrr.Sum(p => p.Amount)
            })
            .OrderBy(mrr => mrr.Month)
            .ToListAsync();
    }

    public async Task<List<CourseSoldResponse>> GetTopSoldCourses()
    {
        return await _context.Payments
            .Where(p => p.Status == "Paid")
            .GroupBy(p => new
            {
                p.Course.Id,
                p.Course.Name,
                p.Course.ThumbnailUrl
            })
            .Select(x => new CourseSoldResponse
            {
                CourseId = x.Key.Id,
                CourseName = x.Key.Name,
                ThumbnailUrl = x.Key.ThumbnailUrl,
                TotalSold = x.Count(),
                TotalRevenue = x.Sum(p => p.Amount)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(3)
            .ToListAsync();
    }

    public async Task<Payment?> GetByOrderCodeAsync(long orderCode)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderCode == orderCode);
    }
}