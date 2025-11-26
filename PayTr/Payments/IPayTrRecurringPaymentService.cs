using PayTr.Models.Payments;

namespace PayTr.Payments;

/// <summary>
/// Kayıtlı kart ile ödeme (recurring payment) servisi
/// </summary>
public interface IPayTrRecurringPaymentService
{
    /// <summary>
    /// Kayıtlı karttan ücret çeker
    /// </summary>
    Task<PayTrPaymentInitResult> ChargeStoredCardAsync(
        PayTrRecurringPaymentRequest request,
        CancellationToken ct = default);
}
