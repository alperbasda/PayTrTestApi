using System.Text.Json.Serialization;

namespace PayTr.Models.PreProvision;

/// <summary>
/// PayTR ön provizyon capture (tahsilat) sonucu
/// </summary>
public sealed class PayTrPreProvisionCaptureResult
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

    /// <summary>reference_no: Referans numarası</summary>
    [JsonPropertyName("reference_no")]
    public string? ReferenceNo { get; set; }

    /// <summary>payment_amount: Capture edilen tutar (kuruş)</summary>
    [JsonPropertyName("payment_amount")]
    public string? PaymentAmount { get; set; }

    /// <summary>test_mode: Test modu</summary>
    [JsonPropertyName("test_mode")]
    public string? TestMode { get; set; }

    /// <summary>İşlem başarılı mı?</summary>
    [JsonIgnore]
    public bool IsSuccessful => Status?.Equals("success", StringComparison.OrdinalIgnoreCase) == true;
}
