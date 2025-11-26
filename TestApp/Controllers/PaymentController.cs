using Microsoft.AspNetCore.Mvc;
using PayTr.Models.Common;
using PayTr.Models.Payments;
using PayTr.Payments;

namespace TestApp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPayTrPaymentService _paymentService;
    private readonly IPayTrCallbackValidator _callbackValidator;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPayTrPaymentService paymentService,
        IPayTrCallbackValidator callbackValidator,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _callbackValidator = callbackValidator;
        _logger = logger;
    }

    /// <summary>
    /// Ödeme baþlatýr (1 TL test ödemesi)
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health([FromBody] InitiatePaymentRequest? request = null)
    {
        return Ok();
    }
    /// <summary>
    /// Ödeme baþlatýr (1 TL test ödemesi)
    /// </summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest? request = null)
    {
        try
        {
            // Test için default deðerler
            var merchantOid = $"1234halil";

            var paymentRequest = new PayTrPaymentInitRequest
            {
                CardHolderName = "PAYTR TEST",
                CardNumber = "9792030394440796",
                ExpiryYear = "30",
                ExpiryMonth = "12",
                Cvv = "000",
                Non3d = false,
                MerchantOid = merchantOid,
                Email = request?.Email ?? "test@example.com",
                PaymentAmount = 1.00m, // 1 TL
                Currency = PayTrCurrency.TL,
                InstallmentCount = 0,
                PaymentType = PayTrPaymentType.Card,

                // IP adresini al
                UserIp = GetClientIpAddress(),

                // Callback URL'leri (kendi sunucunuz için güncelleyin)
                MerchantOkUrl = request?.SuccessUrl ?? "http://localhost:5000/payment-success.html",
                MerchantFailUrl = request?.FailUrl ?? "http://localhost:5000/payment-failed.html",

                // Kullanýcý bilgileri
                UserName = request?.UserName ?? "Test Kullanýcý",
                UserAddress = request?.UserAddress ?? "Test Adres, Ýstanbul",
                UserPhone = request?.UserPhone ?? "05551234567",

                // Sepet (1 TL'lik test ürünü)
                BasketItems = new List<PayTrBasketItem>
                {
                    new()
                    {
                        Name = "Test Ürün - 1 TL",
                        UnitPrice = 1.00m,
                        Quantity = 1
                    }
                },

                // Test ayarlarý
                TestMode = true,
                DebugOn = true,
                SyncMode = false, // Async mode (iframe ile)
                ClientLanguage = PayTrLanguage.Turkish
            };

            _logger.LogInformation("Ödeme baþlatýlýyor. MerchantOid: {MerchantOid}", merchantOid);

            var result = await _paymentService.InitPaymentAsync(paymentRequest);

            if (result.Status == "success" || result.Status == "wait_callback")
            {
                var iframeUrl = result.GetIframeUrl();

                _logger.LogInformation("Ödeme baþarýyla baþlatýldý. Token: {Token}", result.Token);

                return Ok(new
                {
                    success = true,
                    merchantOid = merchantOid,
                    status = result.Status,
                    token = result.Token,
                    iframeUrl = iframeUrl,
                    message = "Ödeme baþarýyla baþlatýldý. IFrame URL'sini kullanarak ödeme sayfasýna yönlendirin."
                });
            }

            _logger.LogWarning("Ödeme baþlatýlamadý. Status: {Status}, Message: {Message}", result.Status, result.Message);

            return BadRequest(new
            {
                success = false,
                status = result.Status,
                message = result.Message ?? "Ödeme baþlatýlamadý"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme baþlatýlýrken hata oluþtu");
            return StatusCode(500, new
            {
                success = false,
                message = "Sunucu hatasý: " + ex.Message
            });
        }
    }

    /// <summary>
    /// PayTR'den gelen callback (bildirim URL)
    /// Bu endpoint'i PayTR panelinde "Bildirim URL" olarak ayarlamalýsýnýz
    /// </summary>
    [HttpPost("callback")]
    public IActionResult PayTrCallback([FromForm] Dictionary<string, string> formData)
    {
        try
        {
            _logger.LogInformation("PayTR callback alýndý. Form data: {@FormData}", formData);

            // Form data'yý payload'a dönüþtür
            var payload = new PayTrPaymentCallbackPayload
            {
                MerchantOid = formData.GetValueOrDefault("merchant_oid", ""),
                Status = formData.GetValueOrDefault("status", ""),
                TotalAmount = int.TryParse(formData.GetValueOrDefault("total_amount", "0"), out var totalAmt) ? totalAmt : 0,
                PaymentAmount = int.TryParse(formData.GetValueOrDefault("payment_amount", "0"), out var payAmt) ? payAmt : 0,
                Currency = formData.GetValueOrDefault("currency", ""),
                TestMode = int.TryParse(formData.GetValueOrDefault("test_mode", "0"), out var testMode) ? testMode : 0,
                Hash = formData.GetValueOrDefault("hash", ""),
                PaymentType = formData.GetValueOrDefault("payment_type", ""),
                InstallmentCount = int.TryParse(formData.GetValueOrDefault("installment_count", "0"), out var instCnt) ? instCnt : 0,
                FailedReasonCode = int.TryParse(formData.GetValueOrDefault("failed_reason_code", "0"), out var failCode) ? failCode : null,
                FailedReasonMessage = formData.GetValueOrDefault("failed_reason_msg", null)
            };

            // Hash doðrulama
            if (!_callbackValidator.TryValidate(payload))
            {
                _logger.LogError("PayTR callback hash doðrulamasý baþarýsýz! MerchantOid: {MerchantOid}", payload.MerchantOid);
                return Ok("PAYTR notification failed: bad hash");
            }

            _logger.LogInformation("PayTR callback hash doðrulamasý baþarýlý. MerchantOid: {MerchantOid}, Status: {Status}",
                payload.MerchantOid, payload.Status);

            // Sipariþ durumu kontrolü (normalde veritabanýndan kontrol edilmeli)
            if (payload.Status == "success")
            {
                // Ödeme baþarýlý
                _logger.LogInformation("Ödeme baþarýlý! MerchantOid: {MerchantOid}, Tutar: {Amount}",
                    payload.MerchantOid, payload.PaymentAmount / 100.0m);

                // TODO: Veritabanýnda sipariþ durumunu güncelle
                // TODO: Gerekirse e-posta/SMS gönder

                return Ok("OK");
            }
            else
            {
                // Ödeme baþarýsýz
                _logger.LogWarning("Ödeme baþarýsýz! MerchantOid: {MerchantOid}, Hata: {ErrorCode} - {ErrorMessage}",
                    payload.MerchantOid, payload.FailedReasonCode, payload.FailedReasonMessage);

                // TODO: Veritabanýnda sipariþ durumunu güncelle (iptal)

                return Ok("OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayTR callback iþlenirken hata oluþtu");
            return Ok("ERROR");
        }
    }

    /// <summary>
    /// Client IP adresini al
    /// </summary>
    private string GetClientIpAddress()
    {
        // X-Forwarded-For header'ýný kontrol et (proxy arkasýndaysa)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        // Remote IP adresini al
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Localhost ise dýþ IP kullan (PayTR localhost kabul etmiyor)
        if (remoteIp == "::1" || remoteIp == "127.0.0.1")
        {
            // Test için geçici bir IP (production'da gerçek IP kullanýlmalý)
            return "85.105.78.56"; // Örnek bir Türkiye IP'si
        }

        return remoteIp ?? "127.0.0.1";
    }
}

/// <summary>
/// Ödeme baþlatma isteði
/// </summary>
public class InitiatePaymentRequest
{
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? UserAddress { get; set; }
    public string? UserPhone { get; set; }
    public string? SuccessUrl { get; set; }
    public string? FailUrl { get; set; }
}
