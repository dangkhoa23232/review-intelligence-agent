using System.Threading.Channels;
using Microsoft.Extensions.Options;
using ReviewIntelligence.Api.Options;

namespace ReviewIntelligence.Api.Services.Ai;

public enum LoaiCongViecAi
{
    /// <summary>Đánh giá vừa DaDuyet → ABSA, vector Qdrant, tóm tắt khía cạnh.</summary>
    PhanTich,

    /// <summary>Đánh giá rời DaDuyet (DaAn) → xóa vector Qdrant, tính lại tóm tắt.</summary>
    GoBo
}

public readonly record struct CongViecAi(int MaDanhGia, LoaiCongViecAi Loai);

/// <summary>Hàng đợi trong bộ nhớ các việc cần gửi ai-service.</summary>
public interface IHangDoiPhanTichDanhGia
{
    /// <summary>Trả false nếu tính năng tắt hoặc hàng đợi đầy.</summary>
    bool DuaVao(int maDanhGia);

    bool DuaVaoGoBo(int maDanhGia);
}

public sealed class HangDoiPhanTichDanhGia(IOptions<AiServiceOptions> options) : IHangDoiPhanTichDanhGia
{
    private readonly Channel<CongViecAi> _kenh = Channel.CreateBounded<CongViecAi>(new BoundedChannelOptions(1_000)
    {
        SingleReader = true,
        FullMode = BoundedChannelFullMode.Wait // TryWrite trả false khi đầy, không chặn request
    });

    public ChannelReader<CongViecAi> Reader => _kenh.Reader;

    public bool DuaVao(int maDanhGia) => Ghi(new CongViecAi(maDanhGia, LoaiCongViecAi.PhanTich));

    public bool DuaVaoGoBo(int maDanhGia) => Ghi(new CongViecAi(maDanhGia, LoaiCongViecAi.GoBo));

    private bool Ghi(CongViecAi viec) => options.Value.BatPhanTich && _kenh.Writer.TryWrite(viec);
}

/// <summary>
/// Đọc hàng đợi và gọi ai-service. ai-service không chạy → chỉ ghi log cảnh báo, không ảnh hưởng luồng duyệt.
/// Việc bị bỏ lỡ được bù bằng POST /api/admin/ai/dong-bo (ai-service quét DaDuyet chưa có kết quả/vector).
/// </summary>
public sealed class PhanTichDanhGiaWorker(
    HangDoiPhanTichDanhGia hangDoi,
    IServiceScopeFactory scopeFactory,
    ILogger<PhanTichDanhGiaWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var viec in hangDoi.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<IAiServiceClient>();
                if (viec.Loai == LoaiCongViecAi.PhanTich)
                    await client.YeuCauPhanTichDanhGiaAsync(viec.MaDanhGia, stoppingToken);
                else
                    await client.YeuCauGoBoDanhGiaAsync(viec.MaDanhGia, stoppingToken);
                logger.LogInformation("Đã gửi {Loai} DanhGia#{MaDanhGia} sang ai-service.", viec.Loai, viec.MaDanhGia);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Không gửi được {Loai} DanhGia#{MaDanhGia} sang ai-service: {Loi}",
                    viec.Loai, viec.MaDanhGia, ex.Message);
            }
        }
    }
}
