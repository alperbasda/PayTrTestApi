namespace PayTr.Models.CardStorage;

/// <summary>
/// PayTR kayıtlı kart listesi isteği (CAPI LIST)
/// </summary>
public sealed class PayTrCardListRequest
{
    /// <summary>utoken: Kullanıcıya özel token</summary>
    public string UserToken { get; set; } = string.Empty;
}
