using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PayTr.Configuration;

namespace PayTr.Security;

/// <summary>
/// PayTR token üretimi ve doğrulama implementasyonu
/// API dökümanlarındaki HMAC-SHA256 algoritmasını kullanır
/// </summary>
public sealed class PayTrTokenGenerator : IPayTrTokenGenerator
{
    private readonly PayTrOptions _options;

    public PayTrTokenGenerator(IOptions<PayTrOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// PayTR token üretir
    /// Örnek: merchant_id + user_ip + merchant_oid + email + payment_amount + ... + merchant_salt
    /// </summary>
    public string GenerateToken(IEnumerable<string> orderedFields)
    {
        if (orderedFields == null)
            throw new ArgumentNullException(nameof(orderedFields));

        // Tüm alanları birleştir
        var concatenated = string.Concat(orderedFields);

        // HMAC-SHA256 ile hash üret
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.MerchantKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated));

        // Base64'e çevir
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Gelen hash'i doğrular (callback validation için)
    /// </summary>
    public bool VerifyHash(string expectedHash, IEnumerable<string> orderedFields)
    {
        if (string.IsNullOrWhiteSpace(expectedHash))
            return false;

        var calculatedHash = GenerateToken(orderedFields);
        return string.Equals(expectedHash, calculatedHash, StringComparison.Ordinal);
    }
}
