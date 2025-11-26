using PayTr.Models.Common;
using System.Text.Json.Serialization;

namespace PayTr.Models.Payments;

/// <summary>
/// PayTR ödeme başlatma cevabı (STEP 1)
/// </summary>
public sealed class PayTrPaymentInitResult
{
    /// <summary>status: "success", "failed", "wait_callback"</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>token: IFrame için kullanılacak token</summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    /// <summary>msg veya message: Açıklama/hata mesajı</summary>
    [JsonPropertyName("reason")]
    public string? Message { get; set; }

    /// <summary>utoken: Kart saklama kullanıcı token'ı</summary>
    [JsonPropertyName("utoken")]
    public string? UserToken { get; set; }

    /// <summary>ctoken: Kart saklama kart token'ı</summary>
    [JsonPropertyName("ctoken")]
    public string? CardToken { get; set; }

    /// <summary>
    /// IFrame URL'sini döndürür (token varsa)
    /// </summary>
    public string? GetIframeUrl(string baseUrl = "https://www.paytr.com")
        => string.IsNullOrEmpty(Token)
            ? null
            : $"{baseUrl.TrimEnd('/')}/odeme/guvenli/{Token}";
}
