using PayTr.Models.Common;

namespace PayTr.Models.Payments;

/// <summary>
/// Kayıtlı kart ile ödeme (recurring payment) isteği
/// </summary>
public sealed class PayTrRecurringPaymentRequest
{
    /// <summary>merchant_oid: Sipariş numarası</summary>
    public string MerchantOid { get; set; } = string.Empty;

    /// <summary>utoken: Kullanıcı token'ı</summary>
    public string UserToken { get; set; } = string.Empty;

    /// <summary>ctoken: Kart token'ı</summary>
    public string CardToken { get; set; } = string.Empty;

    /// <summary>payment_amount: Ödeme tutarı</summary>
    public decimal Amount { get; set; }

    /// <summary>currency: Para birimi</summary>
    public PayTrCurrency Currency { get; set; } = PayTrCurrency.TL;

    /// <summary>installment_count: Taksit sayısı</summary>
    public int? Installment { get; set; }

    /// <summary>user_ip: Kullanıcı IP adresi</summary>
    public string UserIp { get; set; } = string.Empty;

    /// <summary>merchant_ok_url: Başarılı ödeme URL'i</summary>
    public string MerchantOkUrl { get; set; } = string.Empty;

    /// <summary>merchant_fail_url: Hatalı ödeme URL'i</summary>
    public string MerchantFailUrl { get; set; } = string.Empty;
}
