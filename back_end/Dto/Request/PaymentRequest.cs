namespace back_end.Dto.Request;

public class PaymentRequest
{
    public int ItemId { get; set; }
    public decimal Amount { get; set; }
}