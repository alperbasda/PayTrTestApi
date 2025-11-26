using Microsoft.Extensions.Options;
using PayTr.Configuration;
using PayTr.Models.Payments;
using PayTr.Security;

namespace PayTr.Payments;

/// <summary>
/// PayTR callback doğrulama implementasyonu (STEP 2)
/// </summary>
public sealed class PayTrCallbackValidator : IPayTrCallbackValidator
{
    private readonly IPayTrTokenGenerator _tokenGenerator;
    private readonly PayTrOptions _options;

    public PayTrCallbackValidator(
        IPayTrTokenGenerator tokenGenerator,
        IOptions<PayTrOptions> options)
    {
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Callback hash doğrulaması yapar
    /// Format: merchant_oid + merchant_salt + status + total_amount
    /// </summary>
    public bool TryValidate(PayTrPaymentCallbackPayload payload)
    {
        if (payload == null)
            return false;

        // Hash için alanları birleştir
        var fields = new[]
        {
            payload.MerchantOid,
            _options.MerchantSalt,
            payload.Status,
            payload.TotalAmount.ToString()
        };

        // Hash'i doğrula
        return _tokenGenerator.VerifyHash(payload.Hash, fields);
    }
}
