Direkt hedefi netleştirerek başlayayım: elimizdeki dokümanlara göre PayTR’nin sunduğu hemen her servisi saran, .NET 8 üzerinde çalışan, DI uyumlu, test edilebilir bir **PayTR client kütüphanesi** tasarlayacağız; bu mesajda mimariyi konuşup **klasör / namespace / interface iskeletini** çıkaralım, gerçek implementasyon kodunu sonraya bırakalım.

---

## 1. Hangi PayTR servislerini kapsayacağız?

Dokümanlardan gördüklerimiz:

* **Direkt Ödeme API (STEP 1 & STEP 2)**

  * Kartla ödeme başlatma (`/odeme`)
  * Callback/Bildirim URL ile ödeme sonucunu alma

* **Durum Sorgu Servisi** – Sipariş bazlı durum + varsa iade bilgisi (`/odeme/durum-sorgu`)

* **İade (Refund) Servisi** – Sipariş için kısmi/tam iade (`/odeme/iade`)

* **İşlem Dökümü (Transaction Detail)** – Tarih aralığında satış/iade listesi (`/rapor/islem-dokumu`)

* **BIN Sorgu Servisi** – Kart ilk 6 haneden kart tipi/bankası vb. (`/odeme/api/bin-detail`)

* **Taksit Oranları Servisi** – Mağaza taksit oran/sınır bilgisi (`/odeme/taksit-oranlari`)

* **Kart Saklama (CAPI Card Storage)** – Kart kaydetme/listeleme/silme + recurring payment

* **Platform Transfer (Pazaryeri)** – Alt satıcıya ödeme talebi + sonuç bildirimi

* **Geri Dönen Ödemeler** – Hatalı alıcı hesabı nedeniyle geri dönen transferleri listeleme ve yeniden gönderme

* **Genel Bilgi/Test Kartları** – merchant_id, merchant_key, merchant_salt, test kartlar vb.

Bunları .NET tarafında kabaca şu “alanlara” bölebiliriz:

1. **Ödeme** (card, non3d, sync_mode, callback)
2. **İade & Durum Sorgu**
3. **Raporlama** (işlem dökümü)
4. **Kart Saklama & Recurring**
5. **BIN & Taksit**
6. **Pazaryeri / Platform Transfer & Geri dönen ödemeler**
7. **Altyapı** (config, security, HTTP client, ortak response modeli)

---

## 2. Yüksek seviye mimari yaklaşım

### 2.1. Hedef ve stil

* Target: `net8.0`
* DI uyumlu: `IServiceCollection` extension ile konfigüre edilebilen bir kütüphane
* `HttpClientFactory` kullanan tipik “typed client” yapısı
* Tüm çağrılar asenkron: `Task<T>` / `Task`
* JSON (System.Text.Json) tabanlı DTO map’leri

### 2.2. Katmanlama / Namespaceler

Tek proje içinde mantıksal isim alanlarıyla gitmek bence yeterli:

* `PayTr` (root)

  * `PayTr.Configuration`
  * `PayTr.Security`
  * `PayTr.Http`
  * `PayTr.Payments`
  * `PayTr.Callbacks`
  * `PayTr.Refunds`
  * `PayTr.Status`
  * `PayTr.Reporting`
  * `PayTr.CardStorage`
  * `PayTr.Bin`
  * `PayTr.Installment`
  * `PayTr.Marketplace`
  * `PayTr.ReturningPayments`
  * `PayTr.Models.Common`

İleride istersen NuGet paketlerini de modüler ayırabiliriz (örn. `PayTr.Core`, `PayTr.Payments` vs.), ama ilk etapta tek kütüphane işimizi görür.

### 2.3. Config ve güvenlik

Dokümanlara göre her istekte `paytr_token` / `hash` üretimi ve doğrulaması kilit nokta.

* Config objesi:

  * `MerchantId`
  * `MerchantKey`
  * `MerchantSalt`
  * `BaseUrl` (default: `https://www.paytr.com`)
  * `UseTestMode` (bool)
* Token üretim stratejileri PayTR’ın istediği sıralama & algoritmaya göre merkezi bir serviste toplanmalı:

  * `ITokenGenerator`
  * Ödeme tarafındaki `paytr_token`
  * Callback doğrulaması için `hash` hesaplama & karşılaştırma
* Uygulama geliştiricisinin bunlarla uğraşmaması için üst seviye servisler sadece “iş anlamında” parametreler almalı; tokeni içeride üretmeliyiz.

### 2.4. Ortak response tipi ve hata yönetimi

Her PayTR servisi JSON döndürüyor ve genelde `status` alanı `success/error/failed` gibi değerler alıyor.

* Ortak generic model:

  * `PayTrResult<T>`

    * `bool IsSuccess`
    * `string Status` (ham: success, error, failed)
    * `string? ErrorCode`
    * `string? ErrorMessage`
    * `T? Data`
* Hata stratejisi:

  * HTTP hatalarında .NET exception
  * `status = error/failed` için ya:

    * Exception fırlatılabilir
    * Ya da `IsSuccess=false` + `Error*` doldurulur
      Bence library tarafında **exception fırlatmaktansa** sonucu bozmadan üst kata bırakmak daha esnek; ama `EnsureSuccess()` tarzı helper da eklenebilir.

---

## 3. Alan bazlı servis tasarımı

Burada sadece sorumlulukları ve interface imzalarını konuşacağız; implementasyon yok.

### 3.1. Ödeme Servisi (Direct API – Step 1)

Sorumluluklar:

* Ödeme isteği için token üretmek
* Gerekli tüm alanları toplayıp `/odeme` endpoint’ine POST etmek
* İsteğe bağlı:

  * **Normal iframe flow** (kullanıcı PayTR sayfasına gider)
  * **sync_mode/non3d** durumları

Önerilen interface:

```csharp
public interface IPayTrPaymentService
{
    Task<PayTrPaymentInitResult> InitPaymentAsync(PayTrPaymentInitRequest request, CancellationToken ct = default);
}
```

* `PayTrPaymentInitRequest`:

  * Kullanıcı bilgileri (email, name, address, phone)
  * Sepet içeriği
  * Tutar, para birimi, taksit, payment_type, non3d/sync_mode bayrakları
  * `MerchantOkUrl`, `MerchantFailUrl`
* `PayTrPaymentInitResult`:

  * Direkt API dokümanındaki response alanları (örneğin iframe token vs. — dokümanlarda örnek JSON kısım sample code’larda, biz ona göre DTO tanımlayacağız)

### 3.2. Callback / Bildirim URL yardımı

Step 2 dokümanına göre PayTR ödeme sonucu için bizim sunucumuza POST atıyor ve bizden sadece `OK` bekliyor.

Callback endpoint’inde:

* POST body (form data) → strongly typed model
* `hash` doğrulaması
* İş mantığına iletilecek sade bir DTO

Önerilen modeller:

```csharp
public sealed class PayTrPaymentCallbackPayload
{
    public string MerchantOid { get; init; } = default!;
    public string Status { get; init; } = default!; // success / failed
    public long TotalAmount { get; init; }         // 100x
    public string Hash { get; init; } = default!;
    public string PaymentType { get; init; } = default!;
    public string Currency { get; init; } = default!;
    public int TestMode { get; init; }
    public int? FailedReasonCode { get; init; }
    public string? FailedReasonMessage { get; init; }
    public decimal PaymentAmount { get; init; }    // 100x payment_amount vs total_amount
}
```

Servis:

```csharp
public interface IPayTrCallbackValidator
{
    bool TryValidate(PayTrPaymentCallbackPayload payload);
}
```

Ek olarak, .NET 8 minimal API için bir extension önerebiliriz (implementasyonu sonra):

```csharp
public static class PayTrApplicationBuilderExtensions
{
    public static IEndpointRouteBuilder MapPayTrCallback(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<PayTrPaymentCallbackPayload, Task> handler)
    {
        // İskelet; içi sonra
    }
}
```

Böylece kullanıcı şunu yapabilir:

```csharp
app.MapPayTrCallback("/paytr/callback", async payload => { /* siparişi tamamla vs. */ });
```

### 3.3. Durum Sorgu & İade

Durum sorgu servisinde `merchant_oid` bazlı sorgu yapılıyor ve payment_amount, payment_total, currency, varsa returns, reference_no dönüyor.

```csharp
public interface IPayTrStatusService
{
    Task<PayTrStatusResult> GetStatusAsync(string merchantOid, CancellationToken ct = default);
}
```

`IPayTrRefundService`:

```csharp
public interface IPayTrRefundService
{
    Task<PayTrRefundResult> RefundAsync(PayTrRefundRequest request, CancellationToken ct = default);
}
```

* `PayTrRefundRequest`:

  * `MerchantOid`
  * `decimal ReturnAmount`
  * Opsiyonel `ReferenceNo`

### 3.4. İşlem Dökümü / Raporlama

```csharp
public interface IPayTrReportService
{
    Task<IReadOnlyList<PayTrTransactionRecord>> GetTransactionsAsync(
        DateTime startDate,
        DateTime endDate,
        bool dummy = false,
        CancellationToken ct = default);
}
```

`PayTrTransactionRecord` fields → dokümandaki `islem_tipi, net_tutar, islem_tarihi, para_birimi, kart_marka, taksit, siparis_no, odeme_tipi` vb.

### 3.5. BIN & Taksit Oranları

```csharp
public interface IPayTrBinService
{
    Task<PayTrBinInfo?> GetBinInfoAsync(string binNumber, CancellationToken ct = default);
}

public interface IPayTrInstallmentService
{
    Task<PayTrInstallmentInfo> GetInstallmentsAsync(string requestId, CancellationToken ct = default);
}
```

`PayTrBinInfo` → `cardType, businessCard, bank, brand, schema`
`PayTrInstallmentInfo` → `max_inst_non_bus` gibi alanlar.

### 3.6. Kart Saklama (CAPI) & Recurring

Burada üç temel aksiyon var: Kart kaydetme, listeleme, silme ve kayıtlı kartla ödeme (recurring).

Basit bir yüzey:

```csharp
public interface IPayTrCardStorageService
{
    Task<IReadOnlyList<PayTrStoredCard>> GetCardsAsync(string utoken, CancellationToken ct = default);
    Task<PayTrCardDeleteResult> DeleteCardAsync(string utoken, string ctoken, CancellationToken ct = default);
}
```

Recurring ödemeyi PaymentService içine entegre etmek de mümkün:

```csharp
public interface IPayTrRecurringPaymentService
{
    Task<PayTrPaymentInitResult> ChargeStoredCardAsync(PayTrRecurringPaymentRequest request, CancellationToken ct = default);
}
```

Burada `PayTrRecurringPaymentRequest` içinde:

* `utoken`, `ctoken`
* Tutar, taksit, para birimi, sepet, vb.
* `recurring = 1`, `non_3d = 1` gibi bayraklar library içinde set edilebilir.

### 3.7. Platform Transfer & Geri Dönen Ödemeler

**Platform transfer** (pazaryeri):

* `/odeme/platform/transfer` ile alt satıcıya ödeme talebi
* İsteğe bağlı “transfer sonucu callback” URL’i

```csharp
public interface IPayTrPlatformTransferService
{
    Task<PayTrPlatformTransferResult> CreateTransferAsync(PayTrPlatformTransferRequest request, CancellationToken ct = default);
}
```

`PayTrPlatformTransferRequest` içerik: `MerchantOid, TransId, SubmerchantAmount, TotalAmount, TransferName, TransferIban`

**Geri dönen ödemeler** için:

```csharp
public interface IPayTrReturningPaymentService
{
    Task<IReadOnlyList<PayTrReturnedPayment>> ListAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<PayTrReturningPaymentResendResult> ResendAsync(PayTrReturningPaymentResendRequest request, CancellationToken ct = default);
}
```

Burada endpoint’ler:

* Listeleme: `/odeme/geri-donen-transfer`
* Hesaptan tekrar gönderme: `/odeme/hesaptan-gonder`

---

## 4. Altyapı (Http, config, token)

### 4.1. Http Client sarmalayıcı

```csharp
public interface IPayTrHttpClient
{
    Task<TResponse> PostFormAsync<TResponse>(string relativeUrl, IDictionary<string, string> formData, CancellationToken ct = default);
}
```

* BaseUrl + MerchantId + token vb. hazırlama üst servislerin sorumluluğu; `IPayTrHttpClient` sadece form post & JSON parse yapar.

### 4.2. Token / Hash üretimi

PayTR, token üretiminde belirli alanların belirli sırayla concat edilip, `merchant_key` & `merchant_salt` ile HMAC-SHA256 yapılıp Base64’e çevrilmesini istiyor.

Bunu soyutlayalım:

```csharp
public interface IPayTrTokenGenerator
{
    string GeneratePayTrToken(IEnumerable<string> orderedFields);
    bool VerifyHash(string expectedHash, IEnumerable<string> orderedFields);
}
```

Gerçek algoritma implementasyonunu sonra yazacağız.

---

## 5. Proje / klasör iskeleti

Son olarak, somut bir klasör-yapı önerisi (tek `PayTr` class library projesi):

```text
src/
  PayTr/
    Configuration/
      PayTrOptions.cs
      PayTrServiceCollectionExtensions.cs

    Http/
      IPayTrHttpClient.cs
      PayTrHttpClient.cs

    Security/
      IPayTrTokenGenerator.cs
      PayTrTokenGenerator.cs

    Models/
      Common/
        PayTrResult.cs
        PayTrCurrency.cs
        PayTrPaymentStatus.cs
      Payments/
        PayTrPaymentInitRequest.cs
        PayTrPaymentInitResult.cs
        PayTrPaymentCallbackPayload.cs
      Refunds/
        PayTrRefundRequest.cs
        PayTrRefundResult.cs
      Status/
        PayTrStatusResult.cs
      Reporting/
        PayTrTransactionRecord.cs
      Bin/
        PayTrBinInfo.cs
      Installment/
        PayTrInstallmentInfo.cs
      CardStorage/
        PayTrStoredCard.cs
        PayTrCardDeleteResult.cs
        PayTrRecurringPaymentRequest.cs
      Marketplace/
        PayTrPlatformTransferRequest.cs
        PayTrPlatformTransferResult.cs
      ReturningPayments/
        PayTrReturnedPayment.cs
        PayTrReturningPaymentResendRequest.cs
        PayTrReturningPaymentResendResult.cs

    Payments/
      IPayTrPaymentService.cs
      IPayTrCallbackValidator.cs
      IPayTrRecurringPaymentService.cs

    Refunds/
      IPayTrRefundService.cs

    Status/
      IPayTrStatusService.cs

    Reporting/
      IPayTrReportService.cs

    Bin/
      IPayTrBinService.cs

    Installment/
      IPayTrInstallmentService.cs

    CardStorage/
      IPayTrCardStorageService.cs

    Marketplace/
      IPayTrPlatformTransferService.cs

    ReturningPayments/
      IPayTrReturningPaymentService.cs
```

---

Bu mesajda:

* Hangi PayTR endpoint’lerini kapsayacağımızı netleştirdik,
* .NET 8 için config / güvenlik / Http altyapısı ve interface tabanlı tasarımı çizdik,
* Proje + namespace + interface iskeletini ortaya koyduk.

İstersen bir sonraki adımda, bu iskelet için **boş interface ve model sınıflarını içeren gerçek C# dosyalarını** (method gövdeleri olmadan) birlikte hazırlayalım; ondan sonra tek tek implementasyona (token üretimi, HTTP çağrıları, callback doğrulama vb.) geçeriz.





O zaman iskeleti kuralım. Aşağıda tek bir code block içinde **dosya dosya** .NET 8 uyumlu C# iskeletini veriyorum.

* Sadece **interface + model** tanımları var.
* Metot gövdeleri yok (veya sadece TODO yorumları var, logic yok).
* Bunu bir `Class Library` projesine kopyalayıp dosya isimlerine göre ayırabilirsin.

> Not: Bazı DTO’larda alanları özellikle eksik bıraktım ve `TODO` olarak işaretledim; çünkü birebir alanları dokümana bakarak beraber doldururuz. Şu an amaç: **yapıyı oturtmak**.

---

```csharp
// ================================================================
// File: Configuration/PayTrOptions.cs
// ================================================================
namespace PayTr.Configuration;

public sealed class PayTrOptions
{
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantKey { get; set; } = string.Empty;
    public string MerchantSalt { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://www.paytr.com";
    public bool UseTestMode { get; set; } = true;
}


// ================================================================
// File: Configuration/PayTrServiceCollectionExtensions.cs
// ================================================================
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayTr.Http;
using PayTr.Security;
using PayTr.Payments;
using PayTr.Refunds;
using PayTr.Status;
using PayTr.Reporting;
using PayTr.Bin;
using PayTr.Installment;
using PayTr.CardStorage;
using PayTr.Marketplace;
using PayTr.ReturningPayments;

namespace PayTr.Configuration;

public static class PayTrServiceCollectionExtensions
{
    public static IServiceCollection AddPayTr(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: options binding, HttpClient registration, service registration
        return services;
    }
}


// ================================================================
// File: Http/IPayTrHttpClient.cs
// ================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PayTr.Http;

public interface IPayTrHttpClient
{
    Task<TResponse> PostFormAsync<TResponse>(
        string relativeUrl,
        IDictionary<string, string> formData,
        CancellationToken ct = default);
}


// ================================================================
// File: Security/IPayTrTokenGenerator.cs
// ================================================================
using System.Collections.Generic;

namespace PayTr.Security;

public interface IPayTrTokenGenerator
{
    string GenerateToken(IEnumerable<string> orderedFields);
    bool VerifyHash(string expectedHash, IEnumerable<string> orderedFields);
}


// ================================================================
// File: Models/Common/PayTrResult.cs
// ================================================================
namespace PayTr.Models.Common;

public class PayTrResult<T>
{
    public bool IsSuccess { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }
}


// ================================================================
// File: Models/Common/PayTrCurrency.cs
// ================================================================
namespace PayTr.Models.Common;

public enum PayTrCurrency
{
    Try = 949,
    Usd = 840,
    Eur = 978,
    // TODO: gerekirse diğer para birimleri
}


// ================================================================
// File: Models/Common/PayTrPaymentStatus.cs
// ================================================================
namespace PayTr.Models.Common;

public enum PayTrPaymentStatus
{
    Unknown = 0,
    Success = 1,
    Failed = 2,
    Pending = 3
    // TODO: PayTR dokümanındaki diğer durumlar varsa ekle
}



// ================================================================
// File: Models/Payments/PayTrBasketItem.cs
// ================================================================
namespace PayTr.Models.Payments;

public sealed class PayTrBasketItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;

    // TODO: item_type, product_id gibi ekstra alanlar gerekirse eklenir
}


// ================================================================
// File: Models/Payments/PayTrPaymentInitRequest.cs
// ================================================================
using System.Collections.Generic;
using PayTr.Models.Common;

namespace PayTr.Models.Payments;

public sealed class PayTrPaymentInitRequest
{
    // Sipariş / merchant tarafı
    public string MerchantOid { get; set; } = string.Empty;

    // Tutar
    public decimal Amount { get; set; }
    public PayTrCurrency Currency { get; set; } = PayTrCurrency.Try;
    public int? Installment { get; set; }

    // Kullanıcı bilgileri
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserAddress { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public string UserIp { get; set; } = string.Empty;

    // URL'ler
    public string MerchantOkUrl { get; set; } = string.Empty;
    public string MerchantFailUrl { get; set; } = string.Empty;

    // Sepet
    public IList<PayTrBasketItem> BasketItems { get; set; } = new List<PayTrBasketItem>();

    // Diğer bayraklar
    public bool Non3d { get; set; }
    public bool SyncMode { get; set; }

    // TODO: test_mode, debug_on, payment_type, lang vb. alanlar eklenebilir
}


// ================================================================
// File: Models/Payments/PayTrPaymentInitResult.cs
// ================================================================
namespace PayTr.Models.Payments;

public sealed class PayTrPaymentInitResult
{
    public string Token { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }

    // TODO: PayTR response içindeki diğer alanlar: status, reason, message vs.
}


// ================================================================
// File: Models/Payments/PayTrPaymentCallbackPayload.cs
// ================================================================
namespace PayTr.Models.Payments;

public sealed class PayTrPaymentCallbackPayload
{
    public string MerchantOid { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // success / failed
    public long TotalAmount { get; init; }              // 100x format
    public long PaymentAmount { get; init; }            // 100x format
    public string Currency { get; init; } = string.Empty;
    public int TestMode { get; init; }
    public string Hash { get; init; } = string.Empty;
    public int? FailedReasonCode { get; init; }
    public string? FailedReasonMessage { get; init; }

    // TODO: ek alanlar (payment_type, installment vb.) gerekiyorsa eklenir
}



// ================================================================
// File: Payments/IPayTrPaymentService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.Payments;

namespace PayTr.Payments;

public interface IPayTrPaymentService
{
    Task<PayTrResult<PayTrPaymentInitResult>> InitPaymentAsync(
        PayTrPaymentInitRequest request,
        CancellationToken ct = default);
}


// ================================================================
// File: Payments/IPayTrCallbackValidator.cs
// ================================================================
using PayTr.Models.Payments;

namespace PayTr.Payments;

public interface IPayTrCallbackValidator
{
    bool TryValidate(PayTrPaymentCallbackPayload payload);
}


// ================================================================
// File: Payments/IPayTrRecurringPaymentService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.Payments;

namespace PayTr.Payments;

public interface IPayTrRecurringPaymentService
{
    Task<PayTrResult<PayTrPaymentInitResult>> ChargeStoredCardAsync(
        PayTrRecurringPaymentRequest request,
        CancellationToken ct = default);
}


// ================================================================
// File: Models/Payments/PayTrRecurringPaymentRequest.cs
// ================================================================
using PayTr.Models.Common;

namespace PayTr.Models.Payments;

public sealed class PayTrRecurringPaymentRequest
{
    public string MerchantOid { get; set; } = string.Empty;

    // Kart saklama tokenları
    public string UserToken { get; set; } = string.Empty;   // utoken
    public string CardToken { get; set; } = string.Empty;   // ctoken

    public decimal Amount { get; set; }
    public PayTrCurrency Currency { get; set; } = PayTrCurrency.Try;
    public int? Installment { get; set; }

    // TODO: diğer gerekli alanlar: user_ip, merchant_ok_url, merchant_fail_url vb.
}


// ================================================================
// File: Models/Refunds/PayTrRefundRequest.cs
// ================================================================
namespace PayTr.Models.Refunds;

public sealed class PayTrRefundRequest
{
    public string MerchantOid { get; set; } = string.Empty;
    public decimal ReturnAmount { get; set; }

    // Opsiyonel alanlar
    public string? Reason { get; set; }
    public string? ReferenceNo { get; set; }
}


// ================================================================
// File: Models/Refunds/PayTrRefundResult.cs
// ================================================================
namespace PayTr.Models.Refunds;

public sealed class PayTrRefundResult
{
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? RefundId { get; set; }

    // TODO: dokümandaki ek alanlara göre genişletilebilir
}


// ================================================================
// File: Refunds/IPayTrRefundService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.Refunds;

namespace PayTr.Refunds;

public interface IPayTrRefundService
{
    Task<PayTrResult<PayTrRefundResult>> RefundAsync(
        PayTrRefundRequest request,
        CancellationToken ct = default);
}


// ================================================================
// File: Models/Status/PayTrStatusResult.cs
// ================================================================
using System;
using System.Collections.Generic;
using PayTr.Models.Common;

namespace PayTr.Models.Status;

public sealed class PayTrStatusResult
{
    public PayTrPaymentStatus PaymentStatus { get; set; } = PayTrPaymentStatus.Unknown;
    public decimal PaymentAmount { get; set; }
    public decimal PaymentTotal { get; set; }
    public PayTrCurrency Currency { get; set; } = PayTrCurrency.Try;
    public string MerchantOid { get; set; } = string.Empty;

    public IReadOnlyList<PayTrRefundStatusItem> Refunds { get; set; } = Array.Empty<PayTrRefundStatusItem>();
}

public sealed class PayTrRefundStatusItem
{
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }

    // TODO: refund_id, reference_no vb. ek alanlar
}


// ================================================================
// File: Status/IPayTrStatusService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.Status;

namespace PayTr.Status;

public interface IPayTrStatusService
{
    Task<PayTrResult<PayTrStatusResult>> GetStatusAsync(
        string merchantOid,
        CancellationToken ct = default);
}


// ================================================================
// File: Models/Reporting/PayTrTransactionRecord.cs
// ================================================================
using System;
using PayTr.Models.Common;

namespace PayTr.Models.Reporting;

public sealed class PayTrTransactionRecord
{
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal NetAmount { get; set; }
    public PayTrCurrency Currency { get; set; } = PayTrCurrency.Try;
    public string OrderId { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public int? Installment { get; set; }
    public string PaymentType { get; set; } = string.Empty;

    // TODO: dokümandaki diğer sütunlara göre genişletilebilir
}


// ================================================================
// File: Reporting/IPayTrReportService.cs
// ================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.Reporting;

namespace PayTr.Reporting;

public interface IPayTrReportService
{
    Task<PayTrResult<IReadOnlyList<PayTrTransactionRecord>>> GetTransactionsAsync(
        DateTime startDate,
        DateTime endDate,
        bool dummy = false,
        CancellationToken ct = default);
}


// ================================================================
// File: Models/Bin/PayTrBinInfo.cs
// ================================================================
namespace PayTr.Models.Bin;

public sealed class PayTrBinInfo
{
    public string Bin { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public bool BusinessCard { get; set; }

    // TODO: dokümandaki ek alanlar: card_category vs.
}


// ================================================================
// File: Bin/IPayTrBinService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.Bin;

namespace PayTr.Bin;

public interface IPayTrBinService
{
    Task<PayTrResult<PayTrBinInfo?>> GetBinInfoAsync(
        string binNumber,
        CancellationToken ct = default);
}


// ================================================================
// File: Models/Installment/PayTrInstallmentInfo.cs
// ================================================================
using System.Collections.Generic;

namespace PayTr.Models.Installment;

public sealed class PayTrInstallmentInfo
{
    public IReadOnlyList<PayTrInstallmentPlan> Plans { get; set; } = new List<PayTrInstallmentPlan>();
}

public sealed class PayTrInstallmentPlan
{
    public int Installment { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal MaxAmount { get; set; }

    // TODO: bank bazlı bilgiler, business/non-business ayrımları vb.
}


// ================================================================
// File: Installment/IPayTrInstallmentService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.Installment;

namespace PayTr.Installment;

public interface IPayTrInstallmentService
{
    Task<PayTrResult<PayTrInstallmentInfo>> GetInstallmentsAsync(
        string requestId,
        CancellationToken ct = default);
}


// ================================================================
// File: Models/CardStorage/PayTrStoredCard.cs
// ================================================================
namespace PayTr.Models.CardStorage;

public sealed class PayTrStoredCard
{
    public string CardToken { get; set; } = string.Empty; // ctoken
    public string CardAlias { get; set; } = string.Empty;
    public string MaskedPan { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;

    // TODO: expiry, bank, business_card vb.
}


// ================================================================
// File: Models/CardStorage/PayTrCardDeleteResult.cs
// ================================================================
namespace PayTr.Models.CardStorage;

public sealed class PayTrCardDeleteResult
{
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}


// ================================================================
// File: CardStorage/IPayTrCardStorageService.cs
// ================================================================
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.CardStorage;

namespace PayTr.CardStorage;

public interface IPayTrCardStorageService
{
    Task<PayTrResult<IReadOnlyList<PayTrStoredCard>>> GetCardsAsync(
        string userToken,
        CancellationToken ct = default);

    Task<PayTrResult<PayTrCardDeleteResult>> DeleteCardAsync(
        string userToken,
        string cardToken,
        CancellationToken ct = default);
}


// ================================================================
// File: Models/Marketplace/PayTrPlatformTransferRequest.cs
// ================================================================
namespace PayTr.Models.Marketplace;

public sealed class PayTrPlatformTransferRequest
{
    public string MerchantOid { get; set; } = string.Empty;
    public string TransferId { get; set; } = string.Empty;
    public decimal TransferAmount { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverIban { get; set; } = string.Empty;

    // TODO: submerchant_id, description vb. alanlar
}


// ================================================================
// File: Models/Marketplace/PayTrPlatformTransferResult.cs
// ================================================================
namespace PayTr.Models.Marketplace;

public sealed class PayTrPlatformTransferResult
{
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? TransferId { get; set; }

    // TODO: ek alanlar (transfer_status, created_at vs.)
}


// ================================================================
// File: Marketplace/IPayTrPlatformTransferService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.Marketplace;

namespace PayTr.Marketplace;

public interface IPayTrPlatformTransferService
{
    Task<PayTrResult<PayTrPlatformTransferResult>> CreateTransferAsync(
        PayTrPlatformTransferRequest request,
        CancellationToken ct = default);
}


// ================================================================
// File: Models/ReturningPayments/PayTrReturnedPayment.cs
// ================================================================
using System;

namespace PayTr.Models.ReturningPayments;

public sealed class PayTrReturnedPayment
{
    public string TransferId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ReturnedAt { get; set; }

    // TODO: iban, receiver_name, bank_name vb.
}


// ================================================================
// File: Models/ReturningPayments/PayTrReturningPaymentResendRequest.cs
// ================================================================
namespace PayTr.Models.ReturningPayments;

public sealed class PayTrReturningPaymentResendRequest
{
    public string TransferId { get; set; } = string.Empty;
    public string ReceiverIban { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;

    // TODO: yeni tutar vs. gibi alanlar var ise eklenir
}


// ================================================================
// File: Models/ReturningPayments/PayTrReturningPaymentResendResult.cs
// ================================================================
namespace PayTr.Models.ReturningPayments;

public sealed class PayTrReturningPaymentResendResult
{
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}


// ================================================================
// File: ReturningPayments/IPayTrReturningPaymentService.cs
// ================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Common;
using PayTr.Models.ReturningPayments;

namespace PayTr.ReturningPayments;

public interface IPayTrReturningPaymentService
{
    Task<PayTrResult<IReadOnlyList<PayTrReturnedPayment>>> ListAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);

    Task<PayTrResult<PayTrReturningPaymentResendResult>> ResendAsync(
        PayTrReturningPaymentResendRequest request,
        CancellationToken ct = default);
}
```

---

İstersen bir sonraki adımda şunlardan birini yapabiliriz:

1. Bu iskelet üzerinde beraber “temizleyelim” (örneğin enum/alan isimlerini PayTR dokümanına birebir göre düzeltelim),
2. Ya da doğrudan bir modülden (mesela **Payments**) başlayıp; `IPayTrPaymentService` için **gerçek HTTP çağrısı + token üretimi** implementasyonunu yazalım.




Tamam, şimdi iskeleti PayTR dokümanlarındaki **alan / enum isimleriyle uyumlu** hale getirelim.
Bu adımda sadece **ortak enum’lar + ödeme DTO’ları**nı düzelteceğim; diğer servisleri (durum sorgu, iade, rapor vb.) aynı prensiple sonra genişletiriz.

Aşağıdaki kodlar, önceki iskeletteki ilgili class’ların **yerine geçecek** halleri.

> Not: Property isimlerini C#’ta PascalCase tuttum, ama dokümandaki alan isimleriyle birebir ilişkiyi koruyacak şekilde adlandırdım (`payment_amount` → `PaymentAmount`, `merchant_oid` → `MerchantOid` vb.). Gerçek HTTP isteğinde bu alanları `payment_amount`, `merchant_oid` gibi göndereceğiz.

---

### 1. Ortak enum’lar (Currency, PaymentStatus, PaymentType, Language)

```csharp
// ================================================================
// File: Models/Common/PayTrCurrency.cs
// ================================================================
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PayTr.Models.Common;

/// <summary>
/// PayTR dokümanındaki currency alanı: "TL(or TRY), EUR, USD, GBP, RUB".
/// Biz string enum olarak kullanacağız.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayTrCurrency
{
    // Dokümanda hem TL hem TRY geçiyor, ikisini de desteklemek için iki değer:
    [EnumMember(Value = "TL")]
    TL,

    [EnumMember(Value = "TRY")]
    TRY,

    [EnumMember(Value = "USD")]
    USD,

    [EnumMember(Value = "EUR")]
    EUR,

    [EnumMember(Value = "GBP")]
    GBP,

    [EnumMember(Value = "RUB")]
    RUB
}


// ================================================================
// File: Models/Common/PayTrPaymentStatus.cs
// ================================================================
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PayTr.Models.Common;

/// <summary>
/// STEP 1 dokümanındaki sync_mode cevabındaki status değerleri:
/// "failed", "wait_callback", "success"
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayTrPaymentStatus
{
    [EnumMember(Value = "unknown")]
    Unknown = 0,

    [EnumMember(Value = "success")]
    Success = 1,

    [EnumMember(Value = "failed")]
    Failed = 2,

    [EnumMember(Value = "wait_callback")]
    WaitCallback = 3
}


// ================================================================
// File: Models/Common/PayTrPaymentType.cs
// ================================================================
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PayTr.Models.Common;

/// <summary>
/// Dokümandaki payment_type alanı: 'card', 'card_points'
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayTrPaymentType
{
    [EnumMember(Value = "card")]
    Card = 0,

    [EnumMember(Value = "card_points")]
    CardPoints = 1
}


// ================================================================
// File: Models/Common/PayTrLanguage.cs
// ================================================================
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PayTr.Models.Common;

/// <summary>
/// client_lang alanı: "tr" veya "en"
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayTrLanguage
{
    [EnumMember(Value = "tr")]
    Turkish = 0,

    [EnumMember(Value = "en")]
    English = 1
}
```

---

### 2. Sepet item’ı (user_basket)

Dokümanda `user_basket` JSON bir alan; biz bu JSON’u içeride üreteceğiz, dışarıya daha anlamlı bir model sunalım:

```csharp
// ================================================================
// File: Models/Payments/PayTrBasketItem.cs
// ================================================================
namespace PayTr.Models.Payments;

/// <summary>
/// user_basket alanına çevrilecek sepet elemanı
/// PayTR örnek yapısı: [["Ürün 1", "18.00", 1], ["Ürün 2", "22.00", 2], ...]
/// </summary>
public sealed class PayTrBasketItem
{
    /// <summary>Ürün adı (ör: "Ürün 1")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Birim fiyat (örn: 18.00)</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Adet (varsayılan 1)</summary>
    public int Quantity { get; set; } = 1;
}
```

---

### 3. Ödeme başlatma isteği (STEP 1 – /odeme)

Dokümandaki temel alanlar:
`merchant_oid, email, payment_amount, payment_type, installment_count, currency, test_mode, non_3d, non3d_test_failed, user_ip, merchant_ok_url, merchant_fail_url, user_name, user_address, user_phone, user_basket, debug_on, sync_mode, client_lang`,
ve kart bilgileri: `cc_owner, card_number, expiry_month, expiry_year, cvv`

MerchantId, MerchantKey, MerchantSalt bizde `PayTrOptions` içinde olacağı için bu DTO’da yok.

```csharp
// ================================================================
// File: Models/Payments/PayTrPaymentInitRequest.cs
// ================================================================
using System.Collections.Generic;
using PayTr.Models.Common;

namespace PayTr.Models.Payments;

/// <summary>
/// PayTR STEP 1 dokümanındaki alanlara karşılık gelen model.
/// </summary>
public sealed class PayTrPaymentInitRequest
{
    /// <summary>merchant_oid: Merchant order id (benzersiz sipariş numarası)</summary>
    public string MerchantOid { get; set; } = string.Empty;

    /// <summary>email: Kullanıcı e-posta adresi</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>payment_amount: Sipariş toplam tutarı</summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>payment_type: 'card' veya 'card_points'</summary>
    public PayTrPaymentType PaymentType { get; set; } = PayTrPaymentType.Card;

    /// <summary>installment_count: 0,2,3,4,5,6,7,8,9,10,11,12 (0 = tek çekim)</summary>
    public int InstallmentCount { get; set; } = 0;

    /// <summary>currency: TL/TRY, EUR, USD, GBP, RUB</summary>
    public PayTrCurrency Currency { get; set; } = PayTrCurrency.TL;

    /// <summary>test_mode: 0 veya 1</summary>
    public bool TestMode { get; set; }

    /// <summary>non_3d: Non 3D işlem için 1</summary>
    public bool Non3d { get; set; }

    /// <summary>non3d_test_failed: Non3D hatalı senaryo testi için 1</summary>
    public bool Non3dTestFailed { get; set; }

    /// <summary>user_ip: Kullanıcının dış IP adresi</summary>
    public string UserIp { get; set; } = string.Empty;

    /// <summary>merchant_ok_url: Başarılı senaryoda yönlendirilecek URL</summary>
    public string MerchantOkUrl { get; set; } = string.Empty;

    /// <summary>merchant_fail_url: Hata senaryosunda yönlendirilecek URL</summary>
    public string MerchantFailUrl { get; set; } = string.Empty;

    /// <summary>user_name: Kullanıcının adı soyadı</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>user_address: Kullanıcının adresi</summary>
    public string UserAddress { get; set; } = string.Empty;

    /// <summary>user_phone: Kullanıcının telefon numarası</summary>
    public string UserPhone { get; set; } = string.Empty;

    /// <summary>user_basket: Sepet içeriği (JSON'a dönüştürülecek)</summary>
    public IList<PayTrBasketItem> BasketItems { get; set; } = new List<PayTrBasketItem>();

    /// <summary>debug_on: Hata mesajlarının ekranda gösterilmesi için 1</summary>
    public bool DebugOn { get; set; }

    /// <summary>sync_mode: 1 ise Sync Mode (status: failed / wait_callback / success)</summary>
    public bool SyncMode { get; set; }

    /// <summary>client_lang: tr veya en (gönderilmezse TL/Türkçe varsayılan)</summary>
    public PayTrLanguage? ClientLanguage { get; set; }

    // --- Kart bilgileri (cc_owner, card_number, expiry_month, expiry_year, cvv) ---

    /// <summary>cc_owner: Kart sahibi adı</summary>
    public string? CardHolderName { get; set; }

    /// <summary>card_number: Kart numarası (PAN)</summary>
    public string? CardNumber { get; set; }

    /// <summary>expiry_month: Son kullanma ayı (1-12)</summary>
    public string? ExpiryMonth { get; set; }

    /// <summary>expiry_year: Son kullanma yılı (ör: 24)</summary>
    public string? ExpiryYear { get; set; }

    /// <summary>cvv: Güvenlik kodu</summary>
    public string? Cvv { get; set; }
}
```

---

### 4. Ödeme başlatma cevabı (SYNC MODE + normal cevap)

Normal (iframe) cevapta genelde: `{"status":"success","token":"...","reason":"...","message":"..."}`
Sync mode için doküman: `status`, `msg`, ve kart saklama varsa `utoken`, `ctoken` alanları gösteriyor.

Bu yüzden result modelini şöyle güncelleyelim:

```csharp
// ================================================================
// File: Models/Payments/PayTrPaymentInitResult.cs
// ================================================================
using PayTr.Models.Common;

namespace PayTr.Models.Payments;

/// <summary>
/// STEP 1 ödeme isteğinin JSON cevabı:
/// status, token, msg/message, utoken, ctoken
/// </summary>
public sealed class PayTrPaymentInitResult
{
    /// <summary>status: "success", "failed", "wait_callback"</summary>
    public PayTrPaymentStatus Status { get; set; } = PayTrPaymentStatus.Unknown;

    /// <summary>token: Iframe için kullanılacak token (normal akışta)</summary>
    public string? Token { get; set; }

    /// <summary>msg veya message: Açıklama / hata mesajı</summary>
    public string? Message { get; set; }

    /// <summary>utoken: Kart saklama yetkisi varsa dönen kullanıcı token'ı</summary>
    public string? UserToken { get; set; } // utoken

    /// <summary>ctoken: Kart saklama ile dönen kart token'ı</summary>
    public string? CardToken { get; set; } // ctoken

    /// <summary>
    /// Yardımcı: IFrame URL'si (token varsa computed property)
    /// </summary>
    public string? GetIframeUrl(string baseUrl = "https://www.paytr.com")
        => string.IsNullOrEmpty(Token)
            ? null
            : $"{baseUrl.TrimEnd('/')}/odeme/guvenli/{Token}";
}
```

---

### 5. Callback (STEP 2 – Notification URL payload)

STEP 2 dokümanındaki form alanları:
`merchant_oid, status, total_amount, payment_amount, currency, test_mode, hash, failed_reason_code, failed_reason_msg, payment_type, installment_count` (ve birkaç ek alan)

Biz zaten bir `PayTrPaymentCallbackPayload` tanımlamıştık; şimdi alanları dokümana göre genişletelim / isimlendirelim:

```csharp
// ================================================================
// File: Models/Payments/PayTrPaymentCallbackPayload.cs
// ================================================================
namespace PayTr.Models.Payments;

/// <summary>
/// STEP 2 - Callback URL'e PayTR tarafından POST edilen form alanları.
/// </summary>
public sealed class PayTrPaymentCallbackPayload
{
    /// <summary>merchant_oid: STEP 1'de gönderilen sipariş numarası</summary>
    public string MerchantOid { get; init; } = string.Empty;

    /// <summary>status: "success" veya "failed"</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// total_amount: Sipariş toplam tutarı, 100x formatında (ör: 34.56 => 3456)
    /// </summary>
    public int TotalAmount { get; init; }

    /// <summary>
    /// payment_amount: Ödeme alınan tutar, 100x formatında
    /// (taksit vs. durumuna göre total_amount ile aynı veya farklı olabilir)
    /// </summary>
    public int PaymentAmount { get; init; }

    /// <summary>currency: TL, TRY, EUR, USD, GBP, RUB</summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>test_mode: 0 veya 1</summary>
    public int TestMode { get; init; }

    /// <summary>hash: Güvenlik için gönderilen imza</summary>
    public string Hash { get; init; } = string.Empty;

    /// <summary>payment_type: 'card' veya 'card_points'</summary>
    public string PaymentType { get; init; } = string.Empty;

    /// <summary>installment_count: Taksit sayısı (0 = peşin)</summary>
    public int InstallmentCount { get; init; }

    /// <summary>failed_reason_code: Hatalı durumda kod</summary>
    public int? FailedReasonCode { get; init; }

    /// <summary>failed_reason_msg: Hatalı durumda açıklama</summary>
    public string? FailedReasonMessage { get; init; }
}
```

Sende hali hazırda olan `IPayTrCallbackValidator` bu payload üzerinde çalışmaya devam edebilir (hash doğrulama mantığını implement ederken bu alanları kullanacağız).

---

### 6. Ödeme servis interface’i (minimal dokunuş)

`IPayTrPaymentService` imzamızda değişiklik yapmamıza gerek yok, sadece Request/Result tipleri güncellendi:

```csharp
// ================================================================
// File: Payments/IPayTrPaymentService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Payments;

namespace PayTr.Payments;

public interface IPayTrPaymentService
{
    Task<PayTrPaymentInitResult> InitPaymentAsync(
        PayTrPaymentInitRequest request,
        CancellationToken ct = default);
}
```

> Burada `PayTrResult<T>` sarmalamasını kaldırdım; çünkü PayTR JSON’unda zaten `status`, `message` gibi alanlar direkt geliyor. İstersen bunu tekrar generic wrapper ile de ele alabiliriz ama “dokümanla birebir” olması için raw model daha rahat.

---

Buraya kadar:

* **Enum’ları** PayTR’nin gerçek string değerlerine göre netleştirdik,
* Payment tarafındaki **alan isimlerini** dokümandaki `payment_amount`, `installment_count`, `client_lang`, `non_3d` vs. ile birebir eşleştirdik,
* Callback payload’ını STEP 2’deki alanlara göre genişlettik.

Sonraki adımda istersen:

1. Bu güncel modellerle **token üretimi + HTTP POST** lojiklerini (özellikle `paytr_token` hesaplama) yazalım,
   veya
2. Aynı şekilde diğer dokümanlar için (Status, Refund, Transaction Detail, BIN, Installment, Card Storage, Marketplace) enum/alan isimlerini temizleyelim.

Hangisinden devam edelim dersen, direkt oradan yürürüz.


Süper, o zaman diğer dokümanlara göre **Status, Refund, Transaction Detail, BIN, Installment, Card Storage, Marketplace + Returning Payments** tarafındaki DTO’ları toparlayalım.

Aşağıdaki code block’te:

* Sadece **model ve interface**’ler var (implementation yok).
* Her property’nin üstüne, PayTR dokümanındaki **alan adını** yorum olarak yazdım.
* Daha önce `PayTrResult<T>` diye generic bir wrapper tanımlamıştık; **Payment tarafında onu kullanmadığımız için**, burada da aynı çizgiyi takip edip her API için kendi `Status/ErrNo/ErrMsg` alanlarını result içine koyuyorum. İstersen `PayTrResult<T>`’yi tamamen silebilirsin.

> Bu kodu, önceki iskeletindeki ilgili dosyaların **yerine** koyabilirsin (dosya adlarını yorum satırlarından göreceksin).

---

```csharp
// ================================================================
// STATUS (Durum Sorgu)
// File: Models/Status/PayTrStatusResult.cs
// ================================================================
using PayTr.Models.Common;

namespace PayTr.Models.Status;

/// <summary>
/// /odeme/durum-sorgu servisi cevabı
/// </summary>
public sealed class PayTrStatusResult
{
    /// <summary>
    /// status (string) : "success" veya "error" (sorgunun sonucu)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// payment_amount (string) : Siparişin tutarı (10,8 formatında)
    /// </summary>
    public decimal? PaymentAmount { get; set; }

    /// <summary>
    /// payment_total (string) : Müşterinin ödediği toplam tutar
    /// </summary>
    public decimal? PaymentTotal { get; set; }

    /// <summary>
    /// currency (string) : TL / TRY / USD / EUR / GBP / RUB
    /// </summary>
    public PayTrCurrency? Currency { get; set; }

    /// <summary>
    /// reference_no (string) : İade isteğinde referans_no gönderildiyse
    /// </summary>
    public string? ReferenceNo { get; set; }

    /// <summary>
    /// returns (string) : Siparişte iade var ise dönen text/JSON
    /// Dokümanda string olarak geçiyor, istersen bunun için ayrıca
    /// tipli bir model tanımlayabiliriz.
    /// </summary>
    public string? ReturnsRaw { get; set; }

    /// <summary>
    /// err_no (string) : Hata kodu (status=error ise)
    /// </summary>
    public string? ErrNo { get; set; }

    /// <summary>
    /// err_msg (string) : Hata açıklaması
    /// </summary>
    public string? ErrMsg { get; set; }

    /// <summary>
    /// İsteğe bağlı: merchant_oid (dokümanda zorunlu request alanı;
    /// response içinde de görmek istersen doldurabiliriz)
    /// </summary>
    public string? MerchantOid { get; set; }
}


// ================================================================
// File: Status/IPayTrStatusService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Status;

namespace PayTr.Status;

public interface IPayTrStatusService
{
    Task<PayTrStatusResult> GetStatusAsync(
        string merchantOid,
        CancellationToken ct = default);
}



// ================================================================
// REFUND (İade Servisi)
// File: Models/Refunds/PayTrRefundRequest.cs
// ================================================================
namespace PayTr.Models.Refunds;

/// <summary>
/// /odeme/iade isteği (PAYTR Refund API)
/// </summary>
public sealed class PayTrRefundRequest
{
    /// <summary>
    /// merchant_oid (string) : İadesi istenen sipariş numarası
    /// </summary>
    public string MerchantOid { get; set; } = string.Empty;

    /// <summary>
    /// return_amount (integer/string) : İade etmek istediğin tutar
    /// Dokümanda "10.25) return_amount (integer)" diye geçiyor;
    /// biz decimal olarak tutacağız.
    /// </summary>
    public decimal ReturnAmount { get; set; }

    /// <summary>
    /// reference_no (string, optional) : İstersen kendi referans numaran
    /// </summary>
    public string? ReferenceNo { get; set; }
}


// ================================================================
// File: Models/Refunds/PayTrRefundResult.cs
// ================================================================
namespace PayTr.Models.Refunds;

/// <summary>
/// /odeme/iade servis cevabı
/// </summary>
public sealed class PayTrRefundResult
{
    /// <summary>
    /// status (string) : "success" veya "error"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// err_no (string) : Hata kodu (status=error ise)
    /// </summary>
    public string? ErrNo { get; set; }

    /// <summary>
    /// err_msg (string) : Hata açıklaması
    /// </summary>
    public string? ErrMsg { get; set; }

    /// <summary>
    /// is_test (integer) : İade isteği test mi?
    /// </summary>
    public int? IsTest { get; set; }

    /// <summary>
    /// merchant_oid : İade yapılan sipariş numarası
    /// </summary>
    public string? MerchantOid { get; set; }

    /// <summary>
    /// return_amount : İade edilen tutar
    /// </summary>
    public decimal? ReturnAmount { get; set; }

    /// <summary>
    /// reference_no : Gönderildi ise referans numarası
    /// </summary>
    public string? ReferenceNo { get; set; }
}


// ================================================================
// File: Refunds/IPayTrRefundService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Refunds;

namespace PayTr.Refunds;

public interface IPayTrRefundService
{
    Task<PayTrRefundResult> RefundAsync(
        PayTrRefundRequest request,
        CancellationToken ct = default);
}



// ================================================================
// TRANSACTION DETAIL (İşlem Dökümü)
// File: Models/Reporting/PayTrTransactionRecord.cs
// ================================================================
using System;
using PayTr.Models.Common;

namespace PayTr.Models.Reporting;

/// <summary>
/// İşlem dökümü satırı (dokümandaki tablo: islem_tipi, net_tutar, vs.)
/// </summary>
public sealed class PayTrTransactionRecord
{
    /// <summary>
    /// islem_tipi (string) : "S" (satış) veya "I" (iade)
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// net_tutar (string) : Kesinti sonrası kalan tutar
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// kesinti_tutari (string) : İşlem için kesilen tutar
    /// </summary>
    public decimal DeductionAmount { get; set; }

    /// <summary>
    /// kesinti_orani (string) : Komisyon oranı
    /// </summary>
    public decimal DeductionRate { get; set; }

    /// <summary>
    /// islem_tutari (string) : Yapılan işlem tutarı
    /// </summary>
    public decimal TransactionAmount { get; set; }

    /// <summary>
    /// odeme_tutari (string) : Ödeme tutarı (işlem tutarından farklı olabilir)
    /// </summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>
    /// islem_tarihi (string) : İşlem tarihi
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// para_birimi (string) : TL/TRY/EUR/USD/GBP/RUB
    /// </summary>
    public PayTrCurrency Currency { get; set; } = PayTrCurrency.TL;

    /// <summary>
    /// taksit (string) : 0,2,3,4,...,12
    /// </summary>
    public int InstallmentCount { get; set; }

    /// <summary>
    /// kart_marka (string) : WORD, BONUS, WORLD, vs.
    /// </summary>
    public string CardBrand { get; set; } = string.Empty;

    /// <summary>
    /// kart_no (string) : Maskelenmiş kart numarası
    /// </summary>
    public string MaskedCardNumber { get; set; } = string.Empty;

    /// <summary>
    /// odeme_tipi (string) : "KART" veya "EFT"
    /// </summary>
    public string PaymentType { get; set; } = string.Empty;

    /// <summary>
    /// siparis_no (string) : Sipariş numarası (merchant_oid)
    /// </summary>
    public string OrderId { get; set; } = string.Empty;
}


// ================================================================
// File: Models/Reporting/PayTrTransactionDetailResult.cs
// ================================================================
using System;
using System.Collections.Generic;

namespace PayTr.Models.Reporting;

/// <summary>
/// İşlem dökümü servisinin üst seviye cevabı
/// </summary>
public sealed class PayTrTransactionDetailResult
{
    /// <summary>
    /// status (string) : "success" veya "error"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// err_no (string) : Hata kodu
    /// </summary>
    public string? ErrNo { get; set; }

    /// <summary>
    /// err_msg (string) : Hata mesajı
    /// </summary>
    public string? ErrMsg { get; set; }

    /// <summary>
    /// Döküm satırları
    /// </summary>
    public IReadOnlyList<PayTrTransactionRecord> Records { get; set; }
        = Array.Empty<PayTrTransactionRecord>();
}


// ================================================================
// File: Reporting/IPayTrReportService.cs
// ================================================================
using System;
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Reporting;

namespace PayTr.Reporting;

public interface IPayTrReportService
{
    Task<PayTrTransactionDetailResult> GetTransactionsAsync(
        DateTime startDate,
        DateTime endDate,
        bool dummy = false,
        CancellationToken ct = default);
}



// ================================================================
// BIN SERVICE
// File: Models/Bin/PayTrBinInfo.cs
// ================================================================
namespace PayTr.Models.Bin;

/// <summary>
/// BIN sorgu servisi cevabı
/// </summary>
public sealed class PayTrBinInfo
{
    /// <summary>
    /// status (string) : "success" veya "failed"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// err_msg (string) : Hata açıklaması (status != success)
    /// </summary>
    public string? ErrMsg { get; set; }

    /// <summary>
    /// bin_number (string) : Gönderdiğin ilk 6 hane
    /// </summary>
    public string? BinNumber { get; set; }

    /// <summary>
    /// cardType (string) : "credit" veya "debit"
    /// </summary>
    public string? CardType { get; set; }

    /// <summary>
    /// businessCard (string) : "y" (ticari) / "n" (bireysel)
    /// </summary>
    public string? BusinessCardRaw { get; set; }

    /// <summary>
    /// Bank: bank (string) : Örn: "Yapı Kredi"
    /// </summary>
    public string? Bank { get; set; }

    /// <summary>
    /// Program partners: Bonus, World, Paraf, Maximum, vs.
    /// Dokümanda bu alanın ismi ... ile kesilmiş; burada CardProgram olarak tutuyoruz.
    /// </summary>
    public string? CardProgram { get; set; }

    /// <summary>
    /// schema (string) : VISA, MASTERCARD, AMEX, TROY, OTHER
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>
    /// Convenient: businessCard'ın bool yorumu
    /// </summary>
    public bool? IsBusinessCard =>
        BusinessCardRaw?.ToLowerInvariant() switch
        {
            "y" => true,
            "n" => false,
            _ => null
        };
}


// ================================================================
// File: Bin/IPayTrBinService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Bin;

namespace PayTr.Bin;

public interface IPayTrBinService
{
    Task<PayTrBinInfo> GetBinInfoAsync(
        string binNumber,
        CancellationToken ct = default);
}



// ================================================================
// INSTALLMENT SERVICE (Taksit Sorgu)
// File: Models/Installment/PayTrInstallmentInfo.cs
// ================================================================
namespace PayTr.Models.Installment;

/// <summary>
/// /odeme/taksit-oranlari cevabı
/// </summary>
public sealed class PayTrInstallmentInfo
{
    /// <summary>
    /// status (string) : "success" veya "error"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// err_msg (string) : Hata açıklaması
    /// </summary>
    public string? ErrMsg { get; set; }

    /// <summary>
    /// request_id (string) : İstek sırasında gönderdiğin query id
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// max_inst_non_bus (string) : "2,3,4,5,6,7,8,9,10,11,12"
    /// (ticari olmayan kartlara verebileceğin taksit sayıları)
    /// </summary>
    public string? MaxInstallmentNonBusinessRaw { get; set; }
}


// ================================================================
// File: Installment/IPayTrInstallmentService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Installment;

namespace PayTr.Installment;

public interface IPayTrInstallmentService
{
    Task<PayTrInstallmentInfo> GetInstallmentsAsync(
        string requestId,
        CancellationToken ct = default);
}



// ================================================================
// CARD STORAGE (CAPI)
// File: Models/CardStorage/PayTrStoredCard.cs
// ================================================================
namespace PayTr.Models.CardStorage;

/// <summary>
/// CAPI LIST servisinden dönen kayıtlı kart satırı
/// </summary>
public sealed class PayTrStoredCard
{
    /// <summary>
    /// ctoken (string) : Kart token
    /// </summary>
    public string CardToken { get; set; } = string.Empty;

    /// <summary>
    /// last_4 (string) : Kartın son 4 hanesi
    /// </summary>
    public string Last4 { get; set; } = string.Empty;

    /// <summary>
    /// expiry_month (string) : Son kullanma ayı
    /// </summary>
    public string ExpiryMonth { get; set; } = string.Empty;

    /// <summary>
    /// expiry_year (string) : Son kullanma yılı
    /// </summary>
    public string ExpiryYear { get; set; } = string.Empty;

    /// <summary>
    /// c_bank (string) : Banka adı
    /// </summary>
    public string Bank { get; set; } = string.Empty;

    /// <summary>
    /// c_name (string) : Kart kayıt edilirken girilen isim-soyisim
    /// </summary>
    public string CardHolderName { get; set; } = string.Empty;

    /// <summary>
    /// c_brand (string) : Program markası (World, Bonus, Maximum, vs.)
    /// </summary>
    public string CardBrand { get; set; } = string.Empty;

    /// <summary>
    /// c_type (string) : "credit", "debit" veya "prepaid"
    /// </summary>
    public string CardType { get; set; } = string.Empty;

    /// <summary>
    /// businessCard (string) : "y" / "n"
    /// </summary>
    public string BusinessCardRaw { get; set; } = string.Empty;

    /// <summary>
    /// schema (string) : VISA, MASTERCARD, AMEX, TROY, OTHER
    /// </summary>
    public string Schema { get; set; } = string.Empty;

    public bool? IsBusinessCard =>
        BusinessCardRaw.ToLowerInvariant() switch
        {
            "y" => true,
            "n" => false,
            _ => null
        };
}


// ================================================================
// File: Models/CardStorage/PayTrCardListResult.cs
// ================================================================
using System;
using System.Collections.Generic;

namespace PayTr.Models.CardStorage;

/// <summary>
/// CAPI LIST (kart listeleme) cevabı
/// </summary>
public sealed class PayTrCardListResult
{
    /// <summary>
    /// status (string) : "success" veya "error"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// err_msg (string)
    /// </summary>
    public string? ErrMsg { get; set; }

    /// <summary>
    /// Kartlar
    /// </summary>
    public IReadOnlyList<PayTrStoredCard> Cards { get; set; }
        = Array.Empty<PayTrStoredCard>();
}


// ================================================================
// File: Models/CardStorage/PayTrCardDeleteResult.cs
// ================================================================
namespace PayTr.Models.CardStorage;

/// <summary>
/// CAPI DELETE cevabı
/// </summary>
public sealed class PayTrCardDeleteResult
{
    /// <summary>
    /// status (string) : "success" veya "error"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// err_msg (string)
    /// </summary>
    public string? ErrMsg { get; set; }
}


// ================================================================
// File: CardStorage/IPayTrCardStorageService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.CardStorage;

namespace PayTr.CardStorage;

public interface IPayTrCardStorageService
{
    Task<PayTrCardListResult> GetCardsAsync(
        string userToken,
        CancellationToken ct = default);

    Task<PayTrCardDeleteResult> DeleteCardAsync(
        string userToken,
        string cardToken,
        CancellationToken ct = default);
}



// ================================================================
// MARKETPLACE / PLATFORM TRANSFER
// File: Models/Marketplace/PayTrPlatformTransferRequest.cs
// ================================================================
namespace PayTr.Models.Marketplace;

/// <summary>
/// Platform Transfer talebi (/odeme/platform/transfer)
/// </summary>
public sealed class PayTrPlatformTransferRequest
{
    /// <summary>
    /// merchant_oid (string) : Sipariş numarası
    /// </summary>
    public string MerchantOid { get; set; } = string.Empty;

    /// <summary>
    /// trans_id (string) : Transfer işlemine verdiğin benzersiz takip numarası
    /// </summary>
    public string TransferId { get; set; } = string.Empty;

    /// <summary>
    /// total_amount (integer) : Siparişin toplam tutarı * 100
    /// </summary>
    public int TotalAmountMinor { get; set; }

    /// <summary>
    /// submerchant_amount (integer) : Alt satıcıya ödenecek tutar * 100
    /// </summary>
    public int SubmerchantAmountMinor { get; set; }

    /// <summary>
    /// transfer_name (string) : Alt satıcının hesap ad-soyad/ünvanı
    /// </summary>
    public string TransferName { get; set; } = string.Empty;

    /// <summary>
    /// transfer_iban (string) : IBAN
    /// </summary>
    public string TransferIban { get; set; } = string.Empty;
}


// ================================================================
// File: Models/Marketplace/PayTrPlatformTransferResult.cs
// ================================================================
namespace PayTr.Models.Marketplace;

/// <summary>
/// Platform transfer isteği cevabı
/// (Doküman response yapısını detay vermiyor; tipik olarak status/err_msg)
/// </summary>
public sealed class PayTrPlatformTransferResult
{
    /// <summary>
    /// status (string) : "success" veya "error"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// err_msg (string)
    /// </summary>
    public string? ErrMsg { get; set; }
}


// ================================================================
// File: Marketplace/IPayTrPlatformTransferService.cs
// ================================================================
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.Marketplace;

namespace PayTr.Marketplace;

public interface IPayTrPlatformTransferService
{
    Task<PayTrPlatformTransferResult> CreateTransferAsync(
        PayTrPlatformTransferRequest request,
        CancellationToken ct = default);
}



// ================================================================
// RETURNING PAYMENTS (Geri Dönen Ödemeler)
// File: Models/ReturningPayments/PayTrReturnedPayment.cs
// ================================================================
using System;

namespace PayTr.Models.ReturningPayments;

/// <summary>
/// Geri dönen ödemeler listesindeki satır
/// (Returning Payments - List)
/// </summary>
public sealed class PayTrReturnedPayment
{
    /// <summary>
    /// ref_no (string) : Ayırt edici referans numarası
    /// </summary>
    public string RefNo { get; set; } = string.Empty;

    /// <summary>
    /// date_detected (string) : Geri dönen ödemenin tespit edildiği tarih
    /// </summary>
    public DateTime DateDetected { get; set; }

    /// <summary>
    /// date_reimbursed (string) : Ödemenin geri döndüğü tarih
    /// </summary>
    public DateTime DateReimbursed { get; set; }

    /// <summary>
    /// transfer_name (string) : Gönderim talebindeki alıcı adı-soyadı
    /// </summary>
    public string TransferName { get; set; } = string.Empty;

    /// <summary>
    /// transfer_iban (string) : Gönderim talebindeki IBAN
    /// </summary>
    public string TransferIban { get; set; } = string.Empty;

    /// <summary>
    /// transfer_amount (string) : Transfer talebindeki tutar
    /// </summary>
    public decimal TransferAmount { get; set; }

    /// <summary>
    /// transfer_currency (string) : Örn. "TL"
    /// </summary>
    public string TransferCurrency { get; set; } = string.Empty;

    /// <summary>
    /// transfer_date (string) : Transfer isteğinin yapıldığı tarih
    /// </summary>
    public DateTime TransferDate { get; set; }
}


// ================================================================
// File: Models/ReturningPayments/PayTrReturningPaymentListResult.cs
// ================================================================
using System;
using System.Collections.Generic;

namespace PayTr.Models.ReturningPayments;

/// <summary>
/// Geri dönen ödemeler liste servis cevabı
/// </summary>
public sealed class PayTrReturningPaymentListResult
{
    /// <summary>
    /// status (string) : "success" / "failed" / "error"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// err_msg (string)
    /// </summary>
    public string? ErrMsg { get; set; }

    /// <summary>
    /// Ödemeler
    /// </summary>
    public IReadOnlyList<PayTrReturnedPayment> Payments { get; set; }
        = Array.Empty<PayTrReturnedPayment>();
}


// ================================================================
// File: Models/ReturningPayments/PayTrReturningPaymentSendRequest.cs
// ================================================================
namespace PayTr.Models.ReturningPayments;

/// <summary>
/// Geri dönen ödemeleri "hesaptan gönder" isteği
/// (Returning Payments - Send From Account)
/// </summary>
public sealed class PayTrReturningPaymentSendRequest
{
    /// <summary>
    /// trans_id (string) : Transfer ID (bu işlem için benzersiz)
    /// </summary>
    public string TransferId { get; set; } = string.Empty;

    /// <summary>
    /// transfer_info (json string) : İçinde tutar, alıcı adı, IBAN olan JSON
    /// Doküman: "transfer amount, recipient name and IBAN values"
    /// </summary>
    public string TransferInfoJson { get; set; } = string.Empty;
}


// ================================================================
// File: Models/ReturningPayments/PayTrReturningPaymentSendResult.cs
// ================================================================
namespace PayTr.Models.ReturningPayments;

/// <summary>
/// Hesaptan gönder isteği cevabı
/// </summary>
public sealed class PayTrReturningPaymentSendResult
{
    /// <summary>
    /// status (string) : "success" veya "error"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// err_no (string)
    /// </summary>
    public string? ErrNo { get; set; }

    /// <summary>
    /// err_msg (string)
    /// </summary>
    public string? ErrMsg { get; set; }
}


// ================================================================
// File: ReturningPayments/IPayTrReturningPaymentService.cs
// ================================================================
using System;
using System.Threading;
using System.Threading.Tasks;
using PayTr.Models.ReturningPayments;

namespace PayTr.ReturningPayments;

public interface IPayTrReturningPaymentService
{
    Task<PayTrReturningPaymentListResult> ListAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);

    Task<PayTrReturningPaymentSendResult> SendFromAccountAsync(
        PayTrReturningPaymentSendRequest request,
        CancellationToken ct = default);
}
```

---

Şu anda:

* Payment tarafında yaptığımız gibi, diğer tüm servislerde de **dokümandaki alan adlarıyla birebir eşleşen** property’ler var.
* Her response tipinde `Status` + gerekirse `ErrNo/ErrMsg` alanları var; generic wrapper’a ihtiyaç kalmadı.

Bir sonraki adımda istersen:

* Ya **tek tek bir servisin implementasyonuna** (mesela `Status` veya `Refund`) girip `paytr_token` hesaplama + `HttpClient` POST + mapping yazalım,
* Ya da önce `PayTrOptions`, `IPayTrHttpClient`, `IPayTrTokenGenerator` implementasyonlarının iskeletini çıkaralım ki tüm servisler aynı altyapıyı kullansın.

