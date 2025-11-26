using PayTr.Models.Common;

namespace PayTr.Models.PreProvision;

/// <summary>
/// PayTR ön provizyon capture (tahsilat) isteği
/// Ön provizyon ile bloke edilen tutarı tahsil eder
/// </summary>
public sealed class PayTrPreProvisionCaptureRequest
{
    /// <summary>merchant_oid: Sipariş numarası (ön provizyon işlemindeki)</summary>
    public string MerchantOid { get; set; } = string.Empty;

    /// <summary>reference_no: Ön provizyon işleminden dönen referans numarası</summary>
    public string ReferenceNo { get; set; } = string.Empty;

    /// <summary>
    /// Capture amount: TL cinsinden decimal — 100x çevirilecek (kuruş)
    /// </summary>
    public decimal CaptureAmount { get; set; }

    /// <summary>client_ip: Kullanıcının IP adresi</summary>
    public string ClientIp { get; set; } = string.Empty;

    /// <summary>client_lang: Müşteri dili (tr/en)</summary>
    public PayTrLanguage? ClientLanguage { get; set; }

    /// <summary>test_mode: Test modu (0 veya 1)</summary>
    public bool TestMode { get; set; }
}
