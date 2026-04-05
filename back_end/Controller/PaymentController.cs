using back_end.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controller;

[ApiController]
[Route("/payments")]
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
        decimal totalRevenue = await _paymentService.TotalRevenue();
        return Ok(new { totalRevenue = totalRevenue });
    }
}