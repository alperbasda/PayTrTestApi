using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PayTr.Models.Common;

/// <summary>
/// PayTR para birimleri
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayTrCurrency
{
    /// <summary>Türk Lirası (TL)</summary>
    [EnumMember(Value = "TL")]
    TL,

    /// <summary>Türk Lirası (TRY)</summary>
    [EnumMember(Value = "TRY")]
    TRY,

    /// <summary>Amerikan Doları</summary>
    [EnumMember(Value = "USD")]
    USD,

    /// <summary>Euro</summary>
    [EnumMember(Value = "EUR")]
    EUR,

    /// <summary>İngiliz Sterlini</summary>
    [EnumMember(Value = "GBP")]
    GBP,

    /// <summary>Rus Rublesi</summary>
    [EnumMember(Value = "RUB")]
    RUB
}
