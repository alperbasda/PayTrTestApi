using PayTr.Models.Payments;

namespace PayTr.Payments;

/// <summary>
/// PayTR callback doğrulama arayüzü (STEP 2)
/// </summary>
public interface IPayTrCallbackValidator
{
    /// <summary>
    /// Callback payload'ının hash'ini doğrular
    /// </summary>
    bool TryValidate(PayTrPaymentCallbackPayload payload);
}
