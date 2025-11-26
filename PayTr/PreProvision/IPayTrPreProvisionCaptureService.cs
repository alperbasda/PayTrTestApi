using PayTr.Models.PreProvision;

namespace PayTr.PreProvision;

/// <summary>
/// PayTR ön provizyon capture (tahsilat) servisi arayüzü
/// </summary>
public interface IPayTrPreProvisionCaptureService
{
    /// <summary>
    /// Ön provizyon ile bloke edilen tutarı tahsil eder (capture)
    /// </summary>
    /// <param name="request">Capture isteği</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Capture sonucu</returns>
    Task<PayTrPreProvisionCaptureResult> CaptureAsync(
        PayTrPreProvisionCaptureRequest request,
        CancellationToken ct = default);
}
