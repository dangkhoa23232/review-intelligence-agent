namespace ReviewIntelligence.Api.Services.Ai;

/// <summary>HTTP client gọi ai-service (FastAPI).</summary>
public interface IAiServiceClient
{
    /// <summary>
    /// POST {AiService:BaseUrl}/phan-tich/danh-gia  body: {"ma_danh_gia": 123}.
    /// ai-service tự đọc DanhGia từ SQL Server, chạy ABSA, ghi CamXuc/DoTinCay/KetQuaKhiaCanh
    /// và upsert vector Qdrant (DanhGia.MaVectorQdrant).
    /// </summary>
    Task YeuCauPhanTichDanhGiaAsync(int maDanhGia, CancellationToken ct);
}

public class AiServiceClient(HttpClient http) : IAiServiceClient
{
    public async Task YeuCauPhanTichDanhGiaAsync(int maDanhGia, CancellationToken ct)
    {
        using var res = await http.PostAsJsonAsync("phan-tich/danh-gia", new { ma_danh_gia = maDanhGia }, ct);
        res.EnsureSuccessStatusCode();
    }
}
