using System.Text.Json;
using Microsoft.Extensions.Options;
using PayTr.Configuration;
using PayTr.Http;
using PayTr.Models.Payments;
using PayTr.Security;

namespace PayTr.PreAuth;

/// <summary>
/// PayTR ön provizyon (pre-authorization) servisi implementasyonu
/// </summary>
public sealed class PayTrPreAuthService : IPayTrPreAuthService
{
    private readonly IPayTrHttpClient _httpClient;
    private readonly IPayTrTokenGenerator _tokenGenerator;
    private readonly PayTrOptions _options;

    public PayTrPreAuthService(
        IPayTrHttpClient httpClient,
        IPayTrTokenGenerator tokenGenerator,
        IOptions<PayTrOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Ön provizyon işlemi başlatır
    /// </summary>
    public async Task<PayTrPreAuthResult> InitPreAuthAsync(
        PayTrPreAuthRequest request,
        CancellationToken ct = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateRequest(request);

        // Sepeti JSON'a çevir
        var userBasketJson = SerializeBasket(request.BasketItems);

        // Test mode kontrolü
        var testMode = request.TestMode || _options.UseTestMode ? "1" : "0";
        var non3d = request.Non3d ? "1" : "0";
        var non3dTestFailed = request.Non3dTestFailed ? "1" : "0";

        // PayTR token üret
        // Format: merchant_id + user_ip + merchant_oid + email + payment_amount + payment_type + installment_count + currency + test_mode + non_3d + merchant_salt
        var paymentAmountStr = ((int)(request.PaymentAmount * 100)).ToString();
        var installmentCountStr = request.InstallmentCount.ToString();
        var currencyStr = request.Currency.ToString();
        var paymentTypeStr = request.PaymentType == Models.Common.PayTrPaymentType.Card ? "card" : "card_points";

        var tokenFields = new[]
        {
            _options.MerchantId,
            request.UserIp,
            request.MerchantOid,
            request.Email,
            paymentAmountStr,
            paymentTypeStr,
            installmentCountStr,
            currencyStr,
            testMode,
            non3d,
            _options.MerchantSalt
        };

        var paytrToken = _tokenGenerator.GenerateToken(tokenFields);

        // Form data oluştur
        var formData = new Dictionary<string, string>
        {
            ["auth_type"] = "preauth", // ÖNEMLİ: Ön provizyon için
            ["merchant_id"] = _options.MerchantId,
            ["user_ip"] = request.UserIp,
            ["merchant_oid"] = request.MerchantOid,
            ["email"] = request.Email,
            ["payment_amount"] = paymentAmountStr,
            ["payment_type"] = paymentTypeStr,
            ["installment_count"] = installmentCountStr,
            ["currency"] = currencyStr,
            ["test_mode"] = testMode,
            ["non_3d"] = non3d,
            ["user_name"] = request.UserName,
            ["user_address"] = request.UserAddress,
            ["user_phone"] = request.UserPhone,
            ["user_basket"] = userBasketJson,
            ["non3d_test_failed"] = non3dTestFailed,
            ["paytr_token"] = paytrToken,
            // Kart bilgileri (zorunlu)
            ["cc_owner"] = request.CardHolderName,
            ["card_number"] = request.CardNumber,
            ["expiry_month"] = request.ExpiryMonth,
            ["expiry_year"] = request.ExpiryYear,
            ["cvv"] = request.Cvv
        };

        // Opsiyonel alanlar
        if (request.ClientLanguage.HasValue)
        {
            var langStr = request.ClientLanguage.Value == Models.Common.PayTrLanguage.Turkish ? "tr" : "en";
            formData["client_lang"] = langStr;
        }

        if (!string.IsNullOrWhiteSpace(request.CardType))
        {
            formData["card_type"] = request.CardType;
        }

        // 3D Secure için URL'ler (varsa)
        if (!string.IsNullOrWhiteSpace(request.MerchantOkUrl))
        {
            formData["merchant_ok_url"] = request.MerchantOkUrl;
        }

        if (!string.IsNullOrWhiteSpace(request.MerchantFailUrl))
        {
            formData["merchant_fail_url"] = request.MerchantFailUrl;
        }

        // API isteği gönder
        var result = await _httpClient.PostFormAsync<PayTrPreAuthResult>("/odeme/auth", formData, ct);

        return result;
    }

    /// <summary>
    /// İstek validasyonu
    /// </summary>
    private static void ValidateRequest(PayTrPreAuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MerchantOid))
            throw new ArgumentException("MerchantOid gereklidir", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email gereklidir", nameof(request));

        if (request.PaymentAmount <= 0)
            throw new ArgumentException("PaymentAmount sıfırdan büyük olmalıdır", nameof(request));

        if (string.IsNullOrWhiteSpace(request.UserIp))
            throw new ArgumentException("UserIp gereklidir", nameof(request));

        if (string.IsNullOrWhiteSpace(request.CardHolderName))
            throw new ArgumentException("CardHolderName gereklidir", nameof(request));

        if (string.IsNullOrWhiteSpace(request.CardNumber))
            throw new ArgumentException("CardNumber gereklidir", nameof(request));

        if (string.IsNullOrWhiteSpace(request.ExpiryMonth))
            throw new ArgumentException("ExpiryMonth gereklidir", nameof(request));

        if (string.IsNullOrWhiteSpace(request.ExpiryYear))
            throw new ArgumentException("ExpiryYear gereklidir", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Cvv))
            throw new ArgumentException("Cvv gereklidir", nameof(request));

        if (request.BasketItems == null || request.BasketItems.Count == 0)
            throw new ArgumentException("Sepet boş olamaz", nameof(request));
    }

    /// <summary>
    /// Sepeti PayTR formatına çevirir: [["Ürün 1", "18.00", 1], ["Ürün 2", "33.25", 2]]
    /// </summary>
    private static string SerializeBasket(IList<PayTrBasketItem> items)
    {
        var basketArray = items.Select(item => new object[]
        {
            item.Name,
            item.UnitPrice.ToString("F2"),
            item.Quantity
        }).ToArray();

        return JsonSerializer.Serialize(basketArray);
    }
}
