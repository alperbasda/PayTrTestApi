using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PayTr.Models.Common;

/// <summary>
/// PayTR ödeme tipleri
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayTrPaymentType
{
    /// <summary>Kart ile ödeme</summary>
    [EnumMember(Value = "card")]
    Card = 0,

    /// <summary>Kart puanları ile ödeme</summary>
    [EnumMember(Value = "card_points")]
    CardPoints = 1
}
