using PayTr.Models.Common;

namespace PayTr.Models.Payments;

/// <summary>
/// PayTR ödeme başlatma isteği (STEP 1)
/// </summary>
public sealed class PayTrPaymentInitRequest
{
    /// <summary>merchant_oid: Benzersiz sipariş numarası</summary>
    public string MerchantOid { get; set; } = string.Empty;

    /// <summary>email: Müşteri e-posta adresi</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>payment_amount: Sipariş toplam tutarı</summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>payment_type: 'card' veya 'card_points'</summary>
    public PayTrPaymentType PaymentType { get; set; } = PayTrPaymentType.Card;

    /// <summary>installment_count: Taksit sayısı (0 = peşin)</summary>
    public int InstallmentCount { get; set; } = 0;

    /// <summary>currency: Para birimi</summary>
    public PayTrCurrency Currency { get; set; } = PayTrCurrency.TL;

    /// <summary>test_mode: Test modu (0 veya 1)</summary>
    public bool TestMode { get; set; }

    /// <summary>non_3d: Non 3D işlem için 1</summary>
    public bool Non3d { get; set; }

    /// <summary>non3d_test_failed: Non3D hata testi için 1</summary>
    public bool Non3dTestFailed { get; set; }

    /// <summary>user_ip: Kullanıcının IP adresi</summary>
    public string UserIp { get; set; } = string.Empty;

    /// <summary>merchant_ok_url: Başarılı ödeme sonrası URL</summary>
    public string MerchantOkUrl { get; set; } = string.Empty;

    /// <summary>merchant_fail_url: Hatalı ödeme sonrası URL</summary>
    public string MerchantFailUrl { get; set; } = string.Empty;

    /// <summary>user_name: Müşteri adı soyadı</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>user_address: Müşteri adresi</summary>
    public string UserAddress { get; set; } = string.Empty;

    /// <summary>user_phone: Müşteri telefon numarası</summary>
    public string UserPhone { get; set; } = string.Empty;

    /// <summary>user_basket: Sepet içeriği</summary>
    public IList<PayTrBasketItem> BasketItems { get; set; } = new List<PayTrBasketItem>();

    /// <summary>debug_on: Hata mesajlarını göster (1/0)</summary>
    public bool DebugOn { get; set; }

    /// <summary>sync_mode: Senkron mod (1/0)</summary>
    public bool SyncMode { get; set; }

    /// <summary>client_lang: Müşteri dili (tr/en)</summary>
    public PayTrLanguage? ClientLanguage { get; set; }

    // --- Kart bilgileri (opsiyonel, direkt ödeme için) ---

    /// <summary>cc_owner: Kart sahibi adı</summary>
    public string? CardHolderName { get; set; }

    /// <summary>card_number: Kart numarası</summary>
    public string? CardNumber { get; set; }

    /// <summary>expiry_month: Son kullanma ayı (1-12)</summary>
    public string? ExpiryMonth { get; set; }

    /// <summary>expiry_year: Son kullanma yılı (ör: 24)</summary>
    public string? ExpiryYear { get; set; }

    /// <summary>cvv: Güvenlik kodu</summary>
    public string? Cvv { get; set; }
}
