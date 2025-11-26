using Microsoft.Extensions.Options;
using PayTr.Configuration;
using PayTr.Http;
using PayTr.Models.PreProvision;
using PayTr.Security;

namespace PayTr.PreProvision;

/// <summary>
/// PayTR ön provizyon capture (tahsilat) servisi implementasyonu
/// </summary>
public sealed class PayTrPreProvisionCaptureService : IPayTrPreProvisionCaptureService
{
    private readonly IPayTrHttpClient _httpClient;
    private readonly IPayTrTokenGenerator _tokenGenerator;
    private readonly PayTrOptions _options;

    public PayTrPreProvisionCaptureService(
        IPayTrHttpClient httpClient,
        IPayTrTokenGenerator tokenGenerator,
        IOptions<PayTrOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Ön provizyon ile bloke edilen tutarı tahsil eder (capture)
    /// </summary>
    public async Task<PayTrPreProvisionCaptureResult> CaptureAsync(
        PayTrPreProvisionCaptureRequest request,
        CancellationToken ct = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateRequest(request);

        // Test mode kontrolü
        var testMode = request.TestMode || _options.UseTestMode ? "1" : "0";

        // Tutarı kuruşa çevir
        var paymentAmountStr = ((int)(request.CaptureAmount * 100)).ToString();

        // PayTR token üret
        // TODO: Token hesaplama formülü için placeholder
        // Format: merchant_id + client_ip + merchant_oid + reference_no + payment_amount + merchant_salt
        var tokenFields = new[]
        {
            _options.MerchantId,
            request.ClientIp,
            request.MerchantOid,
            request.ReferenceNo,
            paymentAmountStr,
            _options.MerchantSalt
        };

        var paytrToken = _tokenGenerator.GenerateToken(tokenFields);

        // Form data oluştur
        var formData = new Dictionary<string, string>
        {
            ["auth_type"] = "capture", // ÖNEMLİ: Capture işlemi için
            ["merchant_id"] = _options.MerchantId,
            ["paytr_token"] = paytrToken,
            ["merchant_oid"] = request.MerchantOid,
            ["reference_no"] = request.ReferenceNo,
            ["payment_amount"] = paymentAmountStr,
            ["client_ip"] = request.ClientIp,
            ["test_failed"] = testMode
        };

        // Opsiyonel alanlar
        if (request.ClientLanguage.HasValue)
        {
            var langStr = request.ClientLanguage.Value == Models.Common.PayTrLanguage.Turkish ? "tr" : "en";
            formData["client_lang"] = langStr;
        }

        // API isteği gönder
        var result = await _httpClient.PostFormAsync<PayTrPreProvisionCaptureResult>("/odeme/auth", formData, ct);

        return result;
    }

    /// <summary>
    /// İstek validasyonu
    /// </summary>
    private static void ValidateRequest(PayTrPreProvisionCaptureRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MerchantOid))
            throw new ArgumentException("MerchantOid gereklidir", nameof(request));

        if (string.IsNullOrWhiteSpace(request.ReferenceNo))
            throw new ArgumentException("ReferenceNo gereklidir", nameof(request));

        if (request.CaptureAmount <= 0)
            throw new ArgumentException("CaptureAmount sıfırdan büyük olmalıdır", nameof(request));

        if (string.IsNullOrWhiteSpace(request.ClientIp))
            throw new ArgumentException("ClientIp gereklidir", nameof(request));
    }
}
