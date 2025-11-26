# PayTr .NET 9 Client Library

PayTR ödeme sisteminin .NET 9 için geliştirilmiş, modern, DI uyumlu client kütüphanesi.

## Özellikler

- ✅ .NET 9 uyumlu
- ✅ Dependency Injection desteği
- ✅ HttpClientFactory kullanımı
- ✅ Async/await pattern
- ✅ HMAC-SHA256 token üretimi
- ✅ Strongly-typed modeller
- ✅ Payment servisi (STEP 1 & STEP 2 callback validation)
- ✅ Recurring payment (kayıtlı kart ile ödeme)
- 🚧 Diğer servisler (Refund, Status, BIN, Installment, vb.) - TODO

## Kurulum

```bash
dotnet add package PayTr
```

## Kullanım

### 1. Konfigürasyon

**appsettings.json:**

```json
{
  "PayTr": {
    "MerchantId": "XXXXXX",
    "MerchantKey": "YYYYYYYYYYYYYY",
    "MerchantSalt": "ZZZZZZZZZZZZZZ",
    "BaseUrl": "https://www.paytr.com",
    "UseTestMode": true
  }
}
```

**Program.cs:**

```csharp
using PayTr.Configuration;

var builder = WebApplication.CreateBuilder(args);

// PayTR servislerini ekle
builder.Services.AddPayTr(builder.Configuration.GetSection("PayTr"));

var app = builder.Build();
```

### 2. Ödeme Başlatma (STEP 1)

```csharp
using PayTr.Payments;
using PayTr.Models.Payments;
using PayTr.Models.Common;

public class PaymentController : ControllerBase
{
    private readonly IPayTrPaymentService _paymentService;

    public PaymentController(IPayTrPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("initiate-payment")]
    public async Task<IActionResult> InitiatePayment()
    {
        var request = new PayTrPaymentInitRequest
        {
            MerchantOid = Guid.NewGuid().ToString(),
            Email = "musteri@email.com",
            PaymentAmount = 100.50m,
            Currency = PayTrCurrency.TL,
            InstallmentCount = 0,
            UserIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            MerchantOkUrl = "https://siteniz.com/odeme-basarili",
            MerchantFailUrl = "https://siteniz.com/odeme-basarisiz",
            UserName = "Ahmet Yılmaz",
            UserAddress = "İstanbul, Türkiye",
            UserPhone = "05551234567",
            BasketItems = new List<PayTrBasketItem>
            {
                new() { Name = "Ürün 1", UnitPrice = 50.25m, Quantity = 2 }
            },
            TestMode = true,
            DebugOn = true
        };

        var result = await _paymentService.InitPaymentAsync(request);

        if (result.Status == "success")
        {
            // IFrame URL'sini al
            var iframeUrl = result.GetIframeUrl();
            return Ok(new { iframeUrl, token = result.Token });
        }

        return BadRequest(new { message = result.Message });
    }
}
```

### 3. Callback Doğrulama (STEP 2)

```csharp
using PayTr.Payments;
using PayTr.Models.Payments;

[HttpPost("paytr-callback")]
public IActionResult PayTrCallback([FromForm] PayTrPaymentCallbackPayload payload)
{
    // Hash doğrulama
    if (!_callbackValidator.TryValidate(payload))
    {
        return BadRequest("PAYTR notification failed: bad hash");
    }

    // Sipariş durumu kontrolü
    if (payload.Status == "success")
    {
        // Ödeme başarılı - Siparişi onayla
        // TODO: Veri tabanında sipariş durumunu güncelle

        return Ok("OK");
    }
    else
    {
        // Ödeme başarısız - Siparişi iptal et
        // TODO: Veri tabanında sipariş durumunu güncelle
        var errorCode = payload.FailedReasonCode;
        var errorMessage = payload.FailedReasonMessage;

        return Ok("OK");
    }
}
```

### 4. Kayıtlı Kart ile Ödeme (Recurring)

```csharp
using PayTr.Payments;
using PayTr.Models.Payments;

public async Task<IActionResult> ChargeStoredCard()
{
    var request = new PayTrRecurringPaymentRequest
    {
        MerchantOid = Guid.NewGuid().ToString(),
        UserToken = "kullanici-token-buraya",
        CardToken = "kart-token-buraya",
        Amount = 50.00m,
        Currency = PayTrCurrency.TL,
        UserIp = "127.0.0.1",
        MerchantOkUrl = "https://siteniz.com/odeme-basarili",
        MerchantFailUrl = "https://siteniz.com/odeme-basarisiz"
    };

    var result = await _recurringPaymentService.ChargeStoredCardAsync(request);

    return Ok(result);
}
```

## Proje Yapısı

```
PayTr/
├── Configuration/       # DI ve options konfigürasyonu
├── Security/           # Token üretimi ve doğrulama (HMAC-SHA256)
├── Http/               # HTTP client sarmalayıcı
├── Models/             # DTO modelleri
│   ├── Common/        # Enum'lar ve ortak tipler
│   ├── Payments/      # Ödeme modelleri
│   └── ...
├── Payments/          # Ödeme servisleri
├── Refunds/           # İade servisleri (TODO)
├── Status/            # Durum sorgu servisleri (TODO)
├── Reporting/         # Raporlama servisleri (TODO)
├── BinService/        # BIN sorgu servisleri (TODO)
├── Installment/       # Taksit sorgu servisleri (TODO)
├── CardStorage/       # Kart saklama servisleri (TODO)
├── Marketplace/       # Platform transfer servisleri (TODO)
└── ReturningPayments/ # Geri dönen ödemeler servisleri (TODO)
```

## Token Üretim Mekanizması

PayTR, HMAC-SHA256 algoritması kullanarak güvenlik token'ları üretir:

1. **Ödeme Token (STEP 1):**
   ```
   merchant_id + user_ip + merchant_oid + email + payment_amount +
   payment_type + installment_count + currency + test_mode + non_3d + merchant_salt
   ```

2. **Callback Doğrulama (STEP 2):**
   ```
   merchant_oid + merchant_salt + status + total_amount
   ```

3. **BIN Sorgu:**
   ```
   bin_number + merchant_id + merchant_salt
   ```

## Roadmap

- [x] Configuration & DI
- [x] Security (Token Generator)
- [x] HTTP Client
- [x] Payment Service (STEP 1 & STEP 2)
- [x] Recurring Payment
- [ ] Refund Service
- [ ] Status Query Service
- [ ] Transaction Report Service
- [ ] BIN Query Service
- [ ] Installment Query Service
- [ ] Card Storage Service (CAPI)
- [ ] Platform Transfer Service
- [ ] Returning Payments Service
- [ ] Unit Tests
- [ ] Integration Tests

## Geliştirme

```bash
# Proje build
dotnet build

# Test çalıştırma
dotnet test

# NuGet paketi oluşturma
dotnet pack -c Release
```

## Lisans

MIT

## Katkıda Bulunma

Pull request'ler kabul edilir. Büyük değişiklikler için lütfen önce bir issue açın.

## Destek

Sorularınız için GitHub Issues kullanabilirsiniz.
