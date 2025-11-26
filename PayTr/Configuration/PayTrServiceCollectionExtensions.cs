using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PayTr.Http;
using PayTr.Security;
using PayTr.Payments;
using PayTr.Refunds;
using PayTr.Status;
using PayTr.Reporting;
using PayTr.BinService;
using PayTr.Installment;
using PayTr.CardStorage;
using PayTr.Marketplace;
using PayTr.ReturningPayments;

namespace PayTr.Configuration;

/// <summary>
/// PayTR servisleri için DI extension metodları
/// </summary>
public static class PayTrServiceCollectionExtensions
{
    /// <summary>
    /// PayTR servislerini IServiceCollection'a ekler
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration section</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddPayTr(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return AddPayTr(services, configuration.Bind);
    }

    /// <summary>
    /// PayTR servislerini IServiceCollection'a ekler
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Options configurator</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddPayTr(
        this IServiceCollection services,
        Action<PayTrOptions> configureOptions)
    {
        // Options registration
        services.Configure(configureOptions);

        // HttpClient registration
        services.AddHttpClient<IPayTrHttpClient, PayTrHttpClient>();

        // Core services
        services.TryAddSingleton<IPayTrTokenGenerator, PayTrTokenGenerator>();
        services.TryAddScoped<IPayTrHttpClient, PayTrHttpClient>();

        // Business services (sadece implement edilmiş olanlar)
        services.TryAddScoped<IPayTrPaymentService, PayTrPaymentService>();
        services.TryAddScoped<IPayTrCallbackValidator, PayTrCallbackValidator>();
        services.TryAddScoped<IPayTrRecurringPaymentService, PayTrRecurringPaymentService>();

        // TODO: Aşağıdaki servisler henüz implement edilmedi
        // services.TryAddScoped<IPayTrRefundService, PayTrRefundService>();
        // services.TryAddScoped<IPayTrStatusService, PayTrStatusService>();
        // services.TryAddScoped<IPayTrReportService, PayTrReportService>();
        // services.TryAddScoped<IPayTrBinService, PayTrBinService>();
        // services.TryAddScoped<IPayTrInstallmentService, PayTrInstallmentService>();
        // services.TryAddScoped<IPayTrCardStorageService, PayTrCardStorageService>();
        // services.TryAddScoped<IPayTrPlatformTransferService, PayTrPlatformTransferService>();
        // services.TryAddScoped<IPayTrReturningPaymentService, PayTrReturningPaymentService>();

        return services;
    }
}
