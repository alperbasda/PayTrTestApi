using System.Text.Json.Serialization;

namespace PayTr.Models.Payments;

/// <summary>
/// PayTR ödeme callback payload'ı (STEP 2 - Bildirim URL)
/// </summary>
public sealed class PayTrPaymentCallbackPayload
{
    /// <summary>merchant_oid: Sipariş numarası</summary>
    [JsonPropertyName("merchant_oid")]
    public string MerchantOid { get; init; } = string.Empty;

    /// <summary>status: "success" veya "failed"</summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>total_amount: Toplam tutar (100x format, ör: 10.00 => 1000)</summary>
    [JsonPropertyName("total_amount")]
    public int TotalAmount { get; init; }

    /// <summary>payment_amount: Ödeme tutarı (100x format)</summary>
    [JsonPropertyName("payment_amount")]
    public int PaymentAmount { get; init; }

    /// <summary>currency: Para birimi</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>test_mode: Test modu (0 veya 1)</summary>
    [JsonPropertyName("test_mode")]
    public int TestMode { get; init; }

    /// <summary>hash: Güvenlik hash'i</summary>
    [JsonPropertyName("hash")]
    public string Hash { get; init; } = string.Empty;

    /// <summary>payment_type: Ödeme tipi ('card', 'card_points')</summary>
    [JsonPropertyName("payment_type")]
    public string PaymentType { get; init; } = string.Empty;

    /// <summary>installment_count: Taksit sayısı</summary>
    [JsonPropertyName("installment_count")]
    public int InstallmentCount { get; init; }

    /// <summary>failed_reason_code: Hata kodu (failed durumunda)</summary>
    [JsonPropertyName("failed_reason_code")]
    public int? FailedReasonCode { get; init; }

    /// <summary>failed_reason_msg: Hata mesajı (failed durumunda)</summary>
    [JsonPropertyName("failed_reason_msg")]
    public string? FailedReasonMessage { get; init; }
}
