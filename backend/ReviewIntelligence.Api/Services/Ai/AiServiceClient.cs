using System.Net;
using System.Text.Json;

namespace ReviewIntelligence.Api.Services.Ai;

/// <summary>HTTP client gọi ai-service (FastAPI).</summary>
public interface IAiServiceClient
{
    /// <summary>POST /phan-tich/danh-gia {"ma_danh_gia"} – ABSA + Qdrant + TomTatKhiaCanh (xử lý nền, 202).</summary>
    Task YeuCauPhanTichDanhGiaAsync(int maDanhGia, CancellationToken ct);

    /// <summary>DELETE /phan-tich/danh-gia/{id} – đánh giá bị ẩn: xóa vector Qdrant, tính lại tóm tắt (202).</summary>
    Task YeuCauGoBoDanhGiaAsync(int maDanhGia, CancellationToken ct);

    /// <summary>Gọi một endpoint JSON của ai-service và trả nguyên văn (dùng cho các API quản trị chuyển tiếp).</summary>
    Task<KetQuaAi> GuiAsync(HttpMethod method, string path, object? body, CancellationToken ct);
}

/// <summary>Kết quả gọi ai-service: mã HTTP + JSON (null nếu không kết nối được).</summary>
public record KetQuaAi(int MaHttp, JsonElement? DuLieu, string? Loi)
{
    public bool OK => MaHttp is >= 200 and < 300;
}

public class AiServiceClient(HttpClient http, ILogger<AiServiceClient> logger) : IAiServiceClient
{
    public async Task YeuCauPhanTichDanhGiaAsync(int maDanhGia, CancellationToken ct)
    {
        using var res = await http.PostAsJsonAsync("phan-tich/danh-gia", new { ma_danh_gia = maDanhGia }, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task YeuCauGoBoDanhGiaAsync(int maDanhGia, CancellationToken ct)
    {
        using var res = await http.DeleteAsync($"phan-tich/danh-gia/{maDanhGia}", ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task<KetQuaAi> GuiAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(method, path);
        if (body is not null) req.Content = JsonContent.Create(body);
        try
        {
            using var res = await http.SendAsync(req, ct);
            var text = await res.Content.ReadAsStringAsync(ct);
            JsonElement? json = null;
            if (!string.IsNullOrWhiteSpace(text))
            {
                try { json = JsonDocument.Parse(text).RootElement.Clone(); }
                catch (JsonException) { /* phản hồi không phải JSON */ }
            }
            return new KetQuaAi((int)res.StatusCode, json, res.IsSuccessStatusCode ? null : LayThongBao(json) ?? text);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning("Không gọi được ai-service {Path}: {Loi}", path, ex.Message);
            return new KetQuaAi((int)HttpStatusCode.ServiceUnavailable, null,
                "Không kết nối được dịch vụ AI. Hãy kiểm tra ai-service đang chạy (uvicorn app.main:app).");
        }
    }

    /// <summary>FastAPI trả {"detail": "..."} hoặc {"title": "...", "errors": [...]}.</summary>
    private static string? LayThongBao(JsonElement? json)
    {
        if (json is not { ValueKind: JsonValueKind.Object } o) return null;
        if (o.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String) return d.GetString();
        if (o.TryGetProperty("errors", out var e) && e.ValueKind == JsonValueKind.Array && e.GetArrayLength() > 0
            && e[0].TryGetProperty("thong_bao", out var tb)) return tb.GetString();
        return o.TryGetProperty("title", out var t) ? t.GetString() : null;
    }
}
