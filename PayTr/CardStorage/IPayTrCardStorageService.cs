using PayTr.Models.CardStorage;

namespace PayTr.CardStorage;

/// <summary>
/// PayTR kart saklama servisi arayüzü
/// </summary>
public interface IPayTrCardStorageService
{
    /// <summary>
    /// Kullanıcının kayıtlı kart listesini getirir (CAPI LIST)
    /// </summary>
    /// <param name="request">Kart listesi isteği</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Kayıtlı kart listesi</returns>
    Task<PayTrCardListResult> GetStoredCardsAsync(
        PayTrCardListRequest request,
        CancellationToken ct = default);
}
