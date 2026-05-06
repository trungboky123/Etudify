namespace back_end.Dto.Response;

public class PaymentResponse
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; }
    public string Status { get; set; }
    public DateTime PaidAt { get; set; }
}