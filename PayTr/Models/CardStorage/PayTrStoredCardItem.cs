using System.Text.Json.Serialization;

namespace PayTr.Models.CardStorage;

/// <summary>
/// PayTR kayıtlı kart öğesi (CAPI LIST)
/// </summary>
public sealed class PayTrStoredCardItem
{
    /// <summary>ctoken: Kart token'ı</summary>
    [JsonPropertyName("ctoken")]
    public string CardToken { get; set; } = string.Empty;

    /// <summary>last_4: Kartın son 4 hanesi</summary>
    [JsonPropertyName("last_4")]
    public string Last4 { get; set; } = string.Empty;

    /// <summary>require_cvv: CVV gerekli mi? ("0" veya "1")</summary>
    [JsonPropertyName("require_cvv")]
    public string RequireCvvRaw { get; set; } = "0";

    /// <summary>CVV gerekli mi?</summary>
    [JsonIgnore]
    public bool RequireCvv => RequireCvvRaw == "1";

    /// <summary>month: Son kullanma ayı</summary>
    [JsonPropertyName("month")]
    public string? Month { get; set; }

    /// <summary>year: Son kullanma yılı</summary>
    [JsonPropertyName("year")]
    public string? Year { get; set; }

    /// <summary>c_bank: Kart bankası</summary>
    [JsonPropertyName("c_bank")]
    public string? Bank { get; set; }

    /// <summary>c_name: Kart sahibi adı</summary>
    [JsonPropertyName("c_name")]
    public string? CardholderName { get; set; }

    /// <summary>c_brand: Kart markası (axess, bonus, vb.)</summary>
    [JsonPropertyName("c_brand")]
    public string? Brand { get; set; }

    /// <summary>c_type: Kart tipi (credit/debit)</summary>
    [JsonPropertyName("c_type")]
    public string? CardType { get; set; }

    /// <summary>businessCard: Ticari kart mı? ("y"/"n")</summary>
    [JsonPropertyName("businessCard")]
    public string? BusinessCardRaw { get; set; }

    /// <summary>Ticari kart mı?</summary>
    [JsonIgnore]
    public bool? IsBusinessCard =>
        BusinessCardRaw is null
            ? null
            : BusinessCardRaw.Equals("y", StringComparison.OrdinalIgnoreCase);

    /// <summary>schema: Kart şeması (VISA, MASTERCARD, TROY, OTHER)</summary>
    [JsonPropertyName("schema")]
    public string? Schema { get; set; }
}
