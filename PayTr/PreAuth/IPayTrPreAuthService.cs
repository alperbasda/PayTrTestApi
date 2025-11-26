using PayTr.Models.Payments;

namespace PayTr.PreAuth;

/// <summary>
/// PayTR ön provizyon (pre-authorization) servisi arayüzü
/// </summary>
public interface IPayTrPreAuthService
{
    /// <summary>
    /// Ön provizyon işlemi başlatır
    /// </summary>
    /// <param name="request">Ön provizyon isteği</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Ön provizyon sonucu</returns>
    Task<PayTrPreAuthResult> InitPreAuthAsync(
        PayTrPreAuthRequest request,
        CancellationToken ct = default);
}
