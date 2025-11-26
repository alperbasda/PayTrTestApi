using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PayTr.Configuration;
using PayTr.Http;
using PayTr.Models.Payments;
using PayTr.Security;

namespace PayTr.Payments;

/// <summary>
/// PayTR ödeme servisi implementasyonu
/// </summary>
public sealed class PayTrPaymentService : IPayTrPaymentService
{
    private readonly IPayTrHttpClient _httpClient;
    private readonly IPayTrTokenGenerator _tokenGenerator;
    private readonly PayTrOptions _options;

    public PayTrPaymentService(
        IPayTrHttpClient httpClient,
        IPayTrTokenGenerator tokenGenerator,
        IOptions<PayTrOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Ödeme başlatır (STEP 1)
    /// </summary>
    public async Task<PayTrPaymentInitResult> InitPaymentAsync(
        PayTrPaymentInitRequest request,
        CancellationToken ct = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Sepeti JSON'a çevir
        var userBasketJson = SerializeBasket(request.BasketItems);
        userBasketJson = JsonSerializer.Serialize(new object[][] {
            new object[] { "Example Product 1", "1.00", 1 },
      
            }
        );
        // Test mode kontrolü
        var testMode = request.TestMode || _options.UseTestMode ? "1" : "0";
        var non3d = request.Non3d ? "1" : "0";
        var non3dTestFailed = request.Non3dTestFailed ? "1" : "0";
        var debugOn = request.DebugOn ? "1" : "0";
        var syncMode = request.SyncMode ? "1" : "0";

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
            ["merchant_ok_url"] = request.MerchantOkUrl,
            ["merchant_fail_url"] = request.MerchantFailUrl,
            ["user_name"] = request.UserName,
            ["user_address"] = request.UserAddress,
            ["user_phone"] = request.UserPhone,
            ["user_basket"] = userBasketJson,
            ["debug_on"] = debugOn,
            ["non3d_test_failed"] = non3dTestFailed,
            ["sync_mode"] = syncMode,
            ["paytr_token"] = paytrToken,
            ["debug_on"] = "1",
            ["store_card"]="0"
        };

        // Opsiyonel alanlar
        if (request.ClientLanguage.HasValue)
        {
            var langStr = request.ClientLanguage.Value == Models.Common.PayTrLanguage.Turkish ? "tr" : "en";
            formData["client_lang"] = langStr;
        }

        if (!string.IsNullOrWhiteSpace(request.CardHolderName))
            formData["cc_owner"] = request.CardHolderName;

        if (!string.IsNullOrWhiteSpace(request.CardNumber))
            formData["card_number"] = request.CardNumber;

        if (!string.IsNullOrWhiteSpace(request.ExpiryMonth))
            formData["expiry_month"] = request.ExpiryMonth;

        if (!string.IsNullOrWhiteSpace(request.ExpiryYear))
            formData["expiry_year"] = request.ExpiryYear;

        if (!string.IsNullOrWhiteSpace(request.Cvv))
            formData["cvv"] = request.Cvv;

        // API isteği gönder
        var result = await _httpClient.PostFormAsync<PayTrPaymentInitResult>("/odeme", formData, ct);

        return result;
    }

    /// <summary>
    /// Sepeti PayTR formatına çevirir: [["Ürün 1", "18.00", 1], ["Ürün 2", "33.25", 2]]
    /// </summary>
    private static string SerializeBasket(IList<PayTrBasketItem> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Sepet boş olamaz");

        var basketArray = items.Select(item => new object[]
        {
            item.Name,
            item.UnitPrice.ToString("F2"),
            item.Quantity
        }).ToArray();

        return JsonSerializer.Serialize(basketArray);
    }
}
