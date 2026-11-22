using System.Text.Json;

namespace ReviewIntelligence.Api.Infrastructure;

/// <summary>Đọc an toàn các cột NVARCHAR chứa JSON (DanhSachAnh, DanChung, ...). JSON hỏng → mảng rỗng.</summary>
public static class JsonCot
{
    public static IReadOnlyList<string> DocMangChuoi(string? json) => Doc<string>(json);

    public static IReadOnlyList<int> DocMangSo(string? json) => Doc<int>(json);

    private static IReadOnlyList<T> Doc<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<T>>(json) ?? []; }
        catch (JsonException) { return []; }
    }
}
