using System.Text.Json;
using Microsoft.Extensions.Options;
using PayTr.Configuration;
using PayTr.Http;
using PayTr.Models.CardStorage;
using PayTr.Security;

namespace PayTr.CardStorage;

/// <summary>
/// PayTR kart saklama servisi implementasyonu
/// </summary>
public sealed class PayTrCardStorageService : IPayTrCardStorageService
{
    private readonly IPayTrHttpClient _httpClient;
    private readonly IPayTrTokenGenerator _tokenGenerator;
    private readonly PayTrOptions _options;

    public PayTrCardStorageService(
        IPayTrHttpClient httpClient,
        IPayTrTokenGenerator tokenGenerator,
        IOptions<PayTrOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Kullanıcının kayıtlı kart listesini getirir (CAPI LIST)
    /// </summary>
    public async Task<PayTrCardListResult> GetStoredCardsAsync(
        PayTrCardListRequest request,
        CancellationToken ct = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.UserToken))
            throw new ArgumentException("UserToken gereklidir", nameof(request));

        // PayTR token üret
        // Format: merchant_id + utoken + merchant_salt
        var tokenFields = new[]
        {
            _options.MerchantId,
            request.UserToken,
            _options.MerchantSalt
        };

        var paytrToken = _tokenGenerator.GenerateToken(tokenFields);

        // Form data oluştur
        var formData = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["utoken"] = request.UserToken,
            ["paytr_token"] = paytrToken
        };

        try
        {
            // API isteği gönder
            var responseString = await _httpClient.PostFormAsStringAsync("/odeme/capi/list", formData, ct);

            // Response'u parse et
            // Başarılı durumda direkt kart listesi array olarak dönüyor
            // Hata durumunda {"status": "error", "err_msg": "..."} dönüyor

            var result = new PayTrCardListResult();

            // Önce hata kontrolü yap
            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                // Eğer object ise ve status field'ı varsa hata mesajıdır
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("status", out var statusProp))
                {
                    result.Status = statusProp.GetString();
                    if (root.TryGetProperty("err_msg", out var errProp))
                    {
                        result.ErrorMessage = errProp.GetString();
                    }
                    return result;
                }

                // Aksi halde kart listesidir
                if (root.ValueKind == JsonValueKind.Array)
                {
                    var cards = JsonSerializer.Deserialize<List<PayTrStoredCardItem>>(responseString);
                    result.Cards = cards ?? new List<PayTrStoredCardItem>();
                }
            }
            catch (JsonException)
            {
                // Parse hatası
                result.Status = "error";
                result.ErrorMessage = "Response parse edilemedi";
            }

            return result;
        }
        catch (Exception ex)
        {
            return new PayTrCardListResult
            {
                Status = "error",
                ErrorMessage = $"API isteği başarısız: {ex.Message}"
            };
        }
    }
}
