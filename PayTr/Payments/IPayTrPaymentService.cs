using PayTr.Models.Payments;

namespace PayTr.Payments;

/// <summary>
/// PayTR ödeme servisi arayüzü
/// </summary>
public interface IPayTrPaymentService
{
    /// <summary>
    /// Ödeme başlatır (STEP 1)
    /// </summary>
    Task<PayTrPaymentInitResult> InitPaymentAsync(
        PayTrPaymentInitRequest request,
        CancellationToken ct = default);
}
