namespace PayTr.Models.Payments;

/// <summary>
/// Sepet elemanı (user_basket)
/// PayTR formatı: [["Ürün 1", "18.00", 1], ["Ürün 2", "33.25", 2]]
/// </summary>
public sealed class PayTrBasketItem
{
    /// <summary>Ürün adı</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Birim fiyat</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Adet</summary>
    public int Quantity { get; set; } = 1;
}
