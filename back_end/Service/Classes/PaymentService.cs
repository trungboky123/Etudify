using System.Text.Json;
using back_end.Components;
using back_end.Database;
using back_end.Dto.Request;
using back_end.Dto.Response;
using back_end.Entity;
using back_end.Repository.Interfaces;
using back_end.Service.Interfaces;
using Microsoft.Extensions.Localization;

namespace back_end.Service.Classes;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly AppDbContext _context;
    private readonly PayOsService _payOsService;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly IEnrollmentService _enrollmentService;

    public PaymentService(IPaymentRepository paymentRepository, AppDbContext context,  PayOsService payOsService,  IStringLocalizer<Messages> localizer, IEnrollmentService enrollmentService)
    {
        _paymentRepository = paymentRepository;
        _context = context;
        _payOsService = payOsService;
        _localizer = localizer;
        _enrollmentService = enrollmentService;
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

    public async Task<Dictionary<string, object>> CreatePayment(string userId, PaymentRequest request)
    {
        Payment payment = new Payment
        {
            OrderCode = Random.Shared.NextInt64(100000, 1000000),
            UserId = userId,
            CourseId = request.ItemId,
            Amount = request.Amount,
            Method = "QRPAY",
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return await _payOsService.CreatePayment(payment);
    }

    public async Task<string> GetPaymentStatus(long orderCode)
    {
        var body = await _payOsService.GetPayment(orderCode);
        if (body == null || !body.TryGetValue("code", out var codeObj) || codeObj?.ToString() != "00")
        {
            throw new ApiExceptionResponse(_localizer["PaymentNotFound"]);
        }
        
        if (!body.TryGetValue("data", out var dataObj) || dataObj == null)
        {
            throw new ApiExceptionResponse("Invalid payment data");
        }

        var dataJson = dataObj.ToString();
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(dataJson);

        if (data == null ||
            !data.TryGetValue("status", out var statusObj) ||
            statusObj == null)
        {
            throw new ApiExceptionResponse("Missing payment status");
        }

        var status = statusObj.ToString();

        if (string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
        {
            await HandlePaid(orderCode);
            return "PAID";
        }
        
        return "PENDING";
    }

    private async Task HandlePaid(long orderCode)
    {
        var payment = await _paymentRepository.GetByOrderCodeAsync(orderCode);
        if (payment == null)
        {
            throw new ApiExceptionResponse(_localizer["PaymentNotFound"]);
        }

        if (string.Equals(payment.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        
        payment.Status = "PAID";
        payment.PaidAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        await _enrollmentService.Enroll(payment.UserId, payment.CourseId);
    }
}