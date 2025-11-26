using Microsoft.Extensions.Options;
using PayTr.Configuration;
using PayTr.Http;
using PayTr.Models.Payments;
using PayTr.Security;

namespace PayTr.Payments;

/// <summary>
/// Kayıtlı kart ile ödeme servisi implementasyonu
/// </summary>
public sealed class PayTrRecurringPaymentService : IPayTrRecurringPaymentService
{
    private readonly IPayTrHttpClient _httpClient;
    private readonly IPayTrTokenGenerator _tokenGenerator;
    private readonly PayTrOptions _options;

    public PayTrRecurringPaymentService(
        IPayTrHttpClient httpClient,
        IPayTrTokenGenerator tokenGenerator,
        IOptions<PayTrOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Kayıtlı karttan ücret çeker
    /// </summary>
    public async Task<PayTrPaymentInitResult> ChargeStoredCardAsync(
        PayTrRecurringPaymentRequest request,
        CancellationToken ct = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Tutar formatı
        var paymentAmountStr = ((int)(request.Amount * 100)).ToString();
        var installmentStr = request.Installment?.ToString() ?? "0";
        var currencyStr = request.Currency.ToString();

        // Token üret (recurring için farklı format olabilir, dokümana göre ayarlanmalı)
        var tokenFields = new[]
        {
            _options.MerchantId,
            request.UserIp,
            request.MerchantOid,
            request.UserToken,
            request.CardToken,
            paymentAmountStr,
            installmentStr,
            currencyStr,
            _options.MerchantSalt
        };

        var paytrToken = _tokenGenerator.GenerateToken(tokenFields);

        // Form data
        var formData = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["user_ip"] = request.UserIp,
            ["merchant_oid"] = request.MerchantOid,
            ["utoken"] = request.UserToken,
            ["ctoken"] = request.CardToken,
            ["payment_amount"] = paymentAmountStr,
            ["installment_count"] = installmentStr,
            ["currency"] = currencyStr,
            ["merchant_ok_url"] = request.MerchantOkUrl,
            ["merchant_fail_url"] = request.MerchantFailUrl,
            ["recurring"] = "1",
            ["non_3d"] = "1",
            ["paytr_token"] = paytrToken
        };

        // API çağrısı (recurring ödeme için aynı endpoint kullanılıyor olabilir)
        var result = await _httpClient.PostFormAsync<PayTrPaymentInitResult>("/odeme", formData, ct);

        return result;
    }
}
