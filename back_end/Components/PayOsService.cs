using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using back_end.Entity;

namespace back_end.Components;

public class PayOsService
{
    public string ClientId { get; set; }
    public string ApiKey { get; set; }
    public String CheckSumKey { get; set; }
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public PayOsService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        ClientId = _config["PayOS:ClientId"];
        ApiKey = _config["PayOS:ApiKey"];
        CheckSumKey = _config["PayOS:CheckSumKey"];
        _httpClient = httpClient;
    }

    public async Task<Dictionary<string, object>> CreatePayment(Payment payment)
    {
        string returnUrl = "https://localhost:5173/home";
        string cancelUrl = "https://localhost:5173/home";

        string description = $"COURSE_{payment.OrderCode}";
        string dataToSign = $"amount={Convert.ToInt64(payment.Amount).ToString()}" +
                            $"&cancelUrl={cancelUrl}" +
                            $"&description={description}" +
                            $"&orderCode={payment.OrderCode}" +
                            $"&returnUrl={returnUrl}";
        string signature = CreateSignature(dataToSign, CheckSumKey);

        var payload = new
        {
            orderCode = payment.OrderCode,
            amount = payment.Amount,
            description = description,
            cancelUrl = cancelUrl,
            returnUrl = returnUrl,
            signature = signature
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api-merchant.payos.vn/v2/payment-requests");
        request.Headers.Add("x-client-id", ClientId);
        request.Headers.Add("x-api-key", ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );
        
        var response = await _httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);

        if (body == null || !body.ContainsKey("code") || body["code"]?.ToString() != "00")
        {
            throw new Exception("Create PayOS payment failed: " + responseString);
        }
        
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(
            body["data"].ToString()
        );

        return data;
    }

    public async Task<Dictionary<string, object>?> GetPayment(long orderCode)
    {
        var url = $"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-client-id", ClientId);
        request.Headers.Add("x-api-key", ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        return result;
    }

    private string CreateSignature(string data, string key)
    {
        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            byte[] rawHmac = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

            var hex = new StringBuilder();
            foreach (var raw in rawHmac)
            {
                hex.AppendFormat("{0:x2}", raw);
            }

            return hex.ToString();
        }
        catch (Exception e)
        {
            throw new Exception("Error creating signature", e);
        }
    }
}