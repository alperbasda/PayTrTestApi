namespace PayTr.Security;

/// <summary>
/// PayTR token üretimi ve doğrulama arayüzü
/// </summary>
public interface IPayTrTokenGenerator
{
    /// <summary>
    /// PayTR token üretir (HMAC-SHA256 + Base64)
    /// </summary>
    /// <param name="orderedFields">Sıralı alanlar</param>
    /// <returns>Base64 encoded token</returns>
    string GenerateToken(IEnumerable<string> orderedFields);

    /// <summary>
    /// Gelen hash'i doğrular
    /// </summary>
    /// <param name="expectedHash">Beklenen hash</param>
    /// <param name="orderedFields">Sıralı alanlar</param>
    /// <returns>Hash doğruysa true</returns>
    bool VerifyHash(string expectedHash, IEnumerable<string> orderedFields);
}
