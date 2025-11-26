using System.Text.Json.Serialization;

namespace PayTr.Models.CardStorage;

/// <summary>
/// PayTR kayıtlı kart listesi sonucu (CAPI LIST)
/// </summary>
public sealed class PayTrCardListResult
{
    /// <summary>status: Hata durumunda "error" (başarılı durumda dönmeyebilir)</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>err_msg: Hata mesajı</summary>
    [JsonPropertyName("err_msg")]
    public string? ErrorMessage { get; set; }

    /// <summary>Kayıtlı kart listesi (başarılı durumda dolu olur)</summary>
    [JsonIgnore]
    public List<PayTrStoredCardItem> Cards { get; set; } = new();

    /// <summary>İşlem başarılı mı?</summary>
    [JsonIgnore]
    public bool IsSuccessful => Status != "error" && ErrorMessage == null;
}
