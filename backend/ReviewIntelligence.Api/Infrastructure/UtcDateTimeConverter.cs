using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReviewIntelligence.Api.Infrastructure;

/// <summary>
/// Mọi cột DATETIME2 trong DB lưu giờ UTC (SYSUTCDATETIME()) nhưng EF đọc ra Kind = Unspecified.
/// Converter này ghi JSON kèm hậu tố "Z" để client hiển thị đúng múi giờ, và chuẩn hóa input về UTC.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss'Z'"));
}
