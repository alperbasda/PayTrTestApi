using Microsoft.AspNetCore.Mvc;
using PayTr.Models.Common;
using PayTr.Models.Payments;
using PayTr.Models.PreProvision;
using PayTr.Payments;
using PayTr.PreAuth;
using PayTr.PreProvision;

namespace TestApp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPayTrPaymentService _paymentService;
    private readonly IPayTrPreAuthService _preAuthService;
    private readonly IPayTrPreProvisionCaptureService _captureService;
    private readonly IPayTrCallbackValidator _callbackValidator;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPayTrPaymentService paymentService,
        IPayTrPreAuthService preAuthService,
        IPayTrPreProvisionCaptureService captureService,
        IPayTrCallbackValidator callbackValidator,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _preAuthService = preAuthService;
        _captureService = captureService;
        _callbackValidator = callbackValidator;
        _logger = logger;
    }

    /// <summary>
    /// �deme ba�lat�r (1 TL test �demesi)
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health([FromBody] InitiatePaymentRequest? request = null)
    {
        return Ok();
    }
    /// <summary>
    /// �deme ba�lat�r (1 TL test �demesi)
    /// </summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest? request = null)
    {
        try
        {
            // Test i�in default de�erler
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

                // Callback URL'leri (kendi sunucunuz i�in g�ncelleyin)
                MerchantOkUrl = request?.SuccessUrl ?? "http://localhost:5000/payment-success.html",
                MerchantFailUrl = request?.FailUrl ?? "http://localhost:5000/payment-failed.html",

                // Kullan�c� bilgileri
                UserName = request?.UserName ?? "Test Kullan�c�",
                UserAddress = request?.UserAddress ?? "Test Adres, �stanbul",
                UserPhone = request?.UserPhone ?? "05551234567",

                // Sepet (1 TL'lik test �r�n�)
                BasketItems = new List<PayTrBasketItem>
                {
                    new()
                    {
                        Name = "Test �r�n - 1 TL",
                        UnitPrice = 1.00m,
                        Quantity = 1
                    }
                },

                // Test ayarlar�
                TestMode = true,
                DebugOn = true,
                SyncMode = false, // Async mode (iframe ile)
                ClientLanguage = PayTrLanguage.Turkish
            };

            _logger.LogInformation("�deme ba�lat�l�yor. MerchantOid: {MerchantOid}", merchantOid);

            var result = await _paymentService.InitPaymentAsync(paymentRequest);

            if (result.Status == "success" || result.Status == "wait_callback")
            {
                var iframeUrl = result.GetIframeUrl();

                _logger.LogInformation("�deme ba�ar�yla ba�lat�ld�. Token: {Token}", result.Token);

                return Ok(new
                {
                    success = true,
                    merchantOid = merchantOid,
                    status = result.Status,
                    token = result.Token,
                    iframeUrl = iframeUrl,
                    message = "�deme ba�ar�yla ba�lat�ld�. IFrame URL'sini kullanarak �deme sayfas�na y�nlendirin."
                });
            }

            _logger.LogWarning("�deme ba�lat�lamad�. Status: {Status}, Message: {Message}", result.Status, result.Message);

            return BadRequest(new
            {
                success = false,
                status = result.Status,
                message = result.Message ?? "�deme ba�lat�lamad�"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "�deme ba�lat�l�rken hata olu�tu");
            return StatusCode(500, new
            {
                success = false,
                message = "Sunucu hatas�: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Ön provizyon (pre-authorization) işlemi başlatır
    /// </summary>
    [HttpPost("preauth")]
    public async Task<IActionResult> InitiatePreAuth([FromBody] InitiatePreAuthRequest? request = null)
    {
        try
        {
            // Test için default değerler
            var merchantOid = $"1234halil";

            var preAuthRequest = new PayTrPreAuthRequest
            {
                // Kart bilgileri (zorunlu)
                CardHolderName = request?.CardHolderName ?? "PAYTR TEST",
                CardNumber = request?.CardNumber ?? "9792030394440796",
                ExpiryYear = request?.ExpiryYear ?? "30",
                ExpiryMonth = request?.ExpiryMonth ?? "12",
                Cvv = request?.Cvv ?? "000",

                // İşlem bilgileri
                MerchantOid = merchantOid,
                Email = request?.Email ?? "test@example.com",
                PaymentAmount = request?.Amount ?? 10.00m,
                Currency = PayTrCurrency.TL,
                InstallmentCount = request?.InstallmentCount ?? 0,
                PaymentType = PayTrPaymentType.Card,

                // Non-3D veya 3D
                Non3d = request?.Non3d ?? true,

                // IP adresini al
                UserIp = GetClientIpAddress(),

                // 3D için callback URL'leri (isteğe bağlı, 3D secure kullanılırsa gerekli)
                MerchantOkUrl = request?.SuccessUrl,
                MerchantFailUrl = request?.FailUrl,

                // Kullanıcı bilgileri
                UserName = request?.UserName ?? "Test Kullanıcı",
                UserAddress = request?.UserAddress ?? "Test Adres, İstanbul",
                UserPhone = request?.UserPhone ?? "05551234567",

                // Sepet
                BasketItems = new List<PayTrBasketItem>
                {
                    new()
                    {
                        Name = request?.ProductName ?? "Test Ürün - Ön Provizyon",
                        UnitPrice = request?.Amount ?? 10.00m,
                        Quantity = 1
                    }
                },

                // Test ayarları
                TestMode = true,
                ClientLanguage = PayTrLanguage.Turkish,
                CardType = request?.CardType
            };

            _logger.LogInformation("Ön provizyon başlatılıyor. MerchantOid: {MerchantOid}, Tutar: {Amount}",
                merchantOid, preAuthRequest.PaymentAmount);

            var result = await _preAuthService.InitPreAuthAsync(preAuthRequest);

            if (result.IsSuccessful)
            {
                _logger.LogInformation("Ön provizyon başarılı. MerchantOid: {MerchantOid}, TransactionId: {TransactionId}",
                    result.MerchantOid, result.TransactionId);

                return Ok(new
                {
                    success = true,
                    merchantOid = result.MerchantOid,
                    transactionId = result.TransactionId,
                    status = result.Status,
                    paymentAmount = result.PaymentAmount,
                    currency = result.Currency,
                    cardBrand = result.CardBrand,
                    message = "Ön provizyon başarıyla tamamlandı. Bu tutar bloke edildi ve sonradan tahsil edilebilir veya iptal edilebilir."
                });
            }

            _logger.LogWarning("Ön provizyon başarısız. Status: {Status}, Reason: {Reason}",
                result.Status, result.Reason);

            return BadRequest(new
            {
                success = false,
                status = result.Status,
                reason = result.Reason,
                message = result.Reason ?? "Ön provizyon işlemi başarısız oldu"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ön provizyon başlatılırken hata oluştu");
            return StatusCode(500, new
            {
                success = false,
                message = "Sunucu hatası: " + ex.Message
            });
        }
    }

    /// <summary>
    /// Ön provizyon capture (tahsilat) işlemi yapar
    /// Bloke edilen tutarı tahsil eder
    /// </summary>
    [HttpPost("preprovision/capture")]
    public async Task<IActionResult> CapturePreProvision([FromBody] CapturePreProvisionRequest? request = null)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Request body gereklidir"
                });
            }

            if (string.IsNullOrWhiteSpace(request.MerchantOid))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "MerchantOid gereklidir"
                });
            }

            if (string.IsNullOrWhiteSpace(request.ReferenceNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ReferenceNo gereklidir (ön provizyon işleminden dönen)"
                });
            }

            if (request.CaptureAmount <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "CaptureAmount sıfırdan büyük olmalıdır"
                });
            }

            var captureRequest = new PayTrPreProvisionCaptureRequest
            {
                MerchantOid = request.MerchantOid,
                ReferenceNo = request.ReferenceNo,
                CaptureAmount = request.CaptureAmount,
                ClientIp = GetClientIpAddress(),
                TestMode = request.TestMode ?? true,
                ClientLanguage = PayTrLanguage.Turkish
            };

            _logger.LogInformation("Capture işlemi başlatılıyor. MerchantOid: {MerchantOid}, ReferenceNo: {ReferenceNo}, Tutar: {Amount}",
                captureRequest.MerchantOid, captureRequest.ReferenceNo, captureRequest.CaptureAmount);

            var result = await _captureService.CaptureAsync(captureRequest);

            if (result.IsSuccessful)
            {
                _logger.LogInformation("Capture başarılı. MerchantOid: {MerchantOid}, ReferenceNo: {ReferenceNo}",
                    result.MerchantOid, result.ReferenceNo);

                return Ok(new
                {
                    success = true,
                    status = result.Status,
                    merchantOid = result.MerchantOid,
                    referenceNo = result.ReferenceNo,
                    capturedAmount = result.PaymentAmount,
                    message = "Ön provizyon tutarı başarıyla tahsil edildi (capture)"
                });
            }

            _logger.LogWarning("Capture başarısız. Status: {Status}, Reason: {Reason}",
                result.Status, result.Reason);

            return BadRequest(new
            {
                success = false,
                status = result.Status,
                reason = result.Reason,
                message = result.Reason ?? "Capture işlemi başarısız oldu"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Capture işlemi sırasında hata oluştu");
            return StatusCode(500, new
            {
                success = false,
                message = "Sunucu hatası: " + ex.Message
            });
        }
    }

    /// <summary>
    /// PayTR'den gelen callback (bildirim URL)
    /// Bu endpoint'i PayTR panelinde "Bildirim URL" olarak ayarlamal�s�n�z
    /// </summary>
    [HttpPost("callback")]
    public IActionResult PayTrCallback([FromForm] Dictionary<string, string> formData)
    {
        try
        {
            _logger.LogInformation("PayTR callback al�nd�. Form data: {@FormData}", formData);

            // Form data'y� payload'a d�n��t�r
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

            // Hash do�rulama
            if (!_callbackValidator.TryValidate(payload))
            {
                _logger.LogError("PayTR callback hash do�rulamas� ba�ar�s�z! MerchantOid: {MerchantOid}", payload.MerchantOid);
                return Ok("PAYTR notification failed: bad hash");
            }

            _logger.LogInformation("PayTR callback hash do�rulamas� ba�ar�l�. MerchantOid: {MerchantOid}, Status: {Status}",
                payload.MerchantOid, payload.Status);

            // Sipari� durumu kontrol� (normalde veritaban�ndan kontrol edilmeli)
            if (payload.Status == "success")
            {
                // �deme ba�ar�l�
                _logger.LogInformation("�deme ba�ar�l�! MerchantOid: {MerchantOid}, Tutar: {Amount}",
                    payload.MerchantOid, payload.PaymentAmount / 100.0m);

                // TODO: Veritaban�nda sipari� durumunu g�ncelle
                // TODO: Gerekirse e-posta/SMS g�nder

                return Ok("OK");
            }
            else
            {
                // �deme ba�ar�s�z
                _logger.LogWarning("�deme ba�ar�s�z! MerchantOid: {MerchantOid}, Hata: {ErrorCode} - {ErrorMessage}",
                    payload.MerchantOid, payload.FailedReasonCode, payload.FailedReasonMessage);

                // TODO: Veritaban�nda sipari� durumunu g�ncelle (iptal)

                return Ok("OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayTR callback i�lenirken hata olu�tu");
            return Ok("ERROR");
        }
    }

    /// <summary>
    /// Client IP adresini al
    /// </summary>
    private string GetClientIpAddress()
    {
        // X-Forwarded-For header'�n� kontrol et (proxy arkas�ndaysa)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        // Remote IP adresini al
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Localhost ise d�� IP kullan (PayTR localhost kabul etmiyor)
        if (remoteIp == "::1" || remoteIp == "127.0.0.1")
        {
            // Test i�in ge�ici bir IP (production'da ger�ek IP kullan�lmal�)
            return "85.105.78.56"; // �rnek bir T�rkiye IP'si
        }

        return remoteIp ?? "127.0.0.1";
    }
}

/// <summary>
/// �deme ba�latma iste�i
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

/// <summary>
/// Ön provizyon başlatma isteği
/// </summary>
public class InitiatePreAuthRequest
{
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? UserAddress { get; set; }
    public string? UserPhone { get; set; }
    public decimal? Amount { get; set; }
    public string? ProductName { get; set; }
    public int? InstallmentCount { get; set; }
    public bool? Non3d { get; set; }

    // Kart bilgileri
    public string? CardHolderName { get; set; }
    public string? CardNumber { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Cvv { get; set; }
    public string? CardType { get; set; }

    // 3D Secure için (opsiyonel)
    public string? SuccessUrl { get; set; }
    public string? FailUrl { get; set; }
}

/// <summary>
/// Ön provizyon capture (tahsilat) isteği
/// </summary>
public class CapturePreProvisionRequest
{
    /// <summary>Sipariş numarası (ön provizyon işlemindeki)</summary>
    public string MerchantOid { get; set; } = string.Empty;

    /// <summary>Ön provizyon işleminden dönen referans numarası</summary>
    public string ReferenceNo { get; set; } = string.Empty;

    /// <summary>Capture edilecek tutar (TL)</summary>
    public decimal CaptureAmount { get; set; }

    /// <summary>Test modu</summary>
    public bool? TestMode { get; set; }
}
