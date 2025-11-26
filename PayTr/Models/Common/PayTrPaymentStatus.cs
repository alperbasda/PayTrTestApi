using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PayTr.Models.Common;

/// <summary>
/// PayTR ödeme durumları
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayTrPaymentStatus
{
    /// <summary>Bilinmeyen durum</summary>
    [EnumMember(Value = "unknown")]
    Unknown = 0,

    /// <summary>Başarılı</summary>
    [EnumMember(Value = "success")]
    Success = 1,

    /// <summary>Başarısız</summary>
    [EnumMember(Value = "failed")]
    Failed = 2,

    /// <summary>Callback bekleniyor (async mode)</summary>
    [EnumMember(Value = "wait_callback")]
    WaitCallback = 3
}
