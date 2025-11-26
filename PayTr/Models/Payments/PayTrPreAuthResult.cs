using System.Text.Json.Serialization;

namespace PayTr.Models.Payments;

/// <summary>
/// PayTR ön provizyon (pre-authorization) sonucu
/// </summary>
public sealed class PayTrPreAuthResult
{
    /// <summary>status: "success" veya "failed"</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>reason: Hata mesajı (başarısızsa)</summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    /// <summary>merchant_oid: Sipariş numarası</summary>
    [JsonPropertyName("merchant_oid")]
    public string? MerchantOid { get; set; }

    /// <summary>hash: Doğrulama hash'i</summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    /// <summary>payment_amount: Ödeme tutarı (kuruş)</summary>
    [JsonPropertyName("payment_amount")]
    public string? PaymentAmount { get; set; }

    /// <summary>payment_type: Ödeme tipi</summary>
    [JsonPropertyName("payment_type")]
    public string? PaymentType { get; set; }

    /// <summary>currency: Para birimi</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>test_mode: Test modu</summary>
    [JsonPropertyName("test_mode")]
    public string? TestMode { get; set; }

    /// <summary>non_3d: 3D secure kullanılmadı mı</summary>
    [JsonPropertyName("non_3d")]
    public string? Non3d { get; set; }

    /// <summary>card_brand: Kart markası</summary>
    [JsonPropertyName("card_brand")]
    public string? CardBrand { get; set; }

    /// <summary>transaction_id: İşlem ID</summary>
    [JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }

    /// <summary>İşlem başarılı mı?</summary>
    [JsonIgnore]
    public bool IsSuccessful => Status?.Equals("success", StringComparison.OrdinalIgnoreCase) == true;
}
