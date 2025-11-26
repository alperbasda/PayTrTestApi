namespace PayTr.Configuration;

/// <summary>
/// PayTR API entegrasyon ayarları
/// </summary>
public sealed class PayTrOptions
{
    /// <summary>
    /// Mağaza ID (merchant_id)
    /// </summary>
    public string MerchantId { get; set; } = string.Empty;

    /// <summary>
    /// Mağaza Anahtarı (merchant_key) - Token üretiminde kullanılır
    /// </summary>
    public string MerchantKey { get; set; } = string.Empty;

    /// <summary>
    /// Mağaza Tuzu (merchant_salt) - Token üretiminde kullanılır
    /// </summary>
    public string MerchantSalt { get; set; } = string.Empty;

    /// <summary>
    /// PayTR API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.paytr.com";

    /// <summary>
    /// Test modu (1: test, 0: canlı)
    /// </summary>
    public bool UseTestMode { get; set; } = true;
}
