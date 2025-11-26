using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PayTr.Models.Common;

/// <summary>
/// PayTR dil seçenekleri (client_lang)
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayTrLanguage
{
    /// <summary>Türkçe</summary>
    [EnumMember(Value = "tr")]
    Turkish = 0,

    /// <summary>İngilizce</summary>
    [EnumMember(Value = "en")]
    English = 1
}
