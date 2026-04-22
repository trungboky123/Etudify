using System.Security.Claims;
using back_end.Dto.Request;
using back_end.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controller;

[ApiController]
[Route("/api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    
    public  PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("total-revenue")]
    public async Task<IActionResult> GetTotalRevenue()
    {
        var totalRevenue = await _paymentService.TotalRevenue();
        return Ok(new { totalRevenue = totalRevenue });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("monthly-revenue")]
    public async Task<IActionResult> GetMonthlyRevenue()
    {
        var responses = await _paymentService.GetMonthlyRevenue();
        return Ok(responses);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopSoldCourses()
    {
        var responses = await _paymentService.GetTopSoldCourses();
        return Ok(responses);
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var qr = await _paymentService.CreatePayment(userId, request);
        return Ok(qr);
    }

    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus([FromQuery] long orderCode)
    {
        var result = await _paymentService.GetPaymentStatus(orderCode);
        return Ok(result);
    }
}