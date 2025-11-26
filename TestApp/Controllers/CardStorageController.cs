using Microsoft.AspNetCore.Mvc;
using PayTr.CardStorage;
using PayTr.Models.CardStorage;

namespace TestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardStorageController : ControllerBase
{
    private readonly IPayTrCardStorageService _cardStorageService;
    private readonly ILogger<CardStorageController> _logger;

    public CardStorageController(
        IPayTrCardStorageService cardStorageService,
        ILogger<CardStorageController> logger)
    {
        _cardStorageService = cardStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcının kayıtlı kart listesini getirir (CAPI LIST)
    /// </summary>
    [HttpGet("stored-cards/{userToken}")]
    public async Task<IActionResult> GetStoredCards(string userToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userToken))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "UserToken gereklidir"
                });
            }

            var request = new PayTrCardListRequest
            {
                UserToken = userToken
            };

            _logger.LogInformation("Kayıtlı kart listesi getiriliyor. UserToken: {UserToken}", userToken);

            var result = await _cardStorageService.GetStoredCardsAsync(request);

            if (result.IsSuccessful)
            {
                _logger.LogInformation("Kayıtlı kart listesi başarıyla getirildi. Kart sayısı: {Count}", result.Cards.Count);

                return Ok(new
                {
                    success = true,
                    cardCount = result.Cards.Count,
                    cards = result.Cards.Select(c => new
                    {
                        cardToken = c.CardToken,
                        last4 = c.Last4,
                        requireCvv = c.RequireCvv,
                        expiryMonth = c.Month,
                        expiryYear = c.Year,
                        bank = c.Bank,
                        cardholderName = c.CardholderName,
                        brand = c.Brand,
                        cardType = c.CardType,
                        isBusinessCard = c.IsBusinessCard,
                        schema = c.Schema
                    })
                });
            }

            _logger.LogWarning("Kayıtlı kart listesi getirilemedi. Hata: {Error}", result.ErrorMessage);

            return BadRequest(new
            {
                success = false,
                status = result.Status,
                message = result.ErrorMessage ?? "Kayıtlı kart listesi getirilemedi"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıtlı kart listesi getirilirken hata oluştu");
            return StatusCode(500, new
            {
                success = false,
                message = "Sunucu hatası: " + ex.Message
            });
        }
    }

    /// <summary>
    /// POST ile kayıtlı kart listesi getirme (alternatif)
    /// </summary>
    [HttpPost("stored-cards")]
    public async Task<IActionResult> GetStoredCardsPost([FromBody] GetStoredCardsRequest? request = null)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserToken))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "UserToken gereklidir"
                });
            }

            return await GetStoredCards(request.UserToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıtlı kart listesi getirilirken hata oluştu");
            return StatusCode(500, new
            {
                success = false,
                message = "Sunucu hatası: " + ex.Message
            });
        }
    }
}

/// <summary>
/// Kayıtlı kart listesi getirme isteği
/// </summary>
public class GetStoredCardsRequest
{
    /// <summary>Kullanıcıya özel token (utoken)</summary>
    public string UserToken { get; set; } = string.Empty;
}
