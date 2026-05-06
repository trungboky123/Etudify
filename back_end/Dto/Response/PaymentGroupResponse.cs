namespace back_end.Dto.Response;

public class PaymentGroupResponse
{
    public DateOnly Date { get; set; }
    public List<PaymentResponse> Payments { get; set; }
}