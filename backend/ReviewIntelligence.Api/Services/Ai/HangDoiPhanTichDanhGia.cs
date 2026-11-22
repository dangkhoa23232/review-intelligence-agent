using System.Threading.Channels;
using Microsoft.Extensions.Options;
using ReviewIntelligence.Api.Options;

namespace ReviewIntelligence.Api.Services.Ai;

/// <summary>Hàng đợi trong bộ nhớ các MaDanhGia cần gửi ai-service phân tích.</summary>
public interface IHangDoiPhanTichDanhGia
{
    /// <summary>Trả false nếu tính năng tắt hoặc hàng đợi đầy.</summary>
    bool DuaVao(int maDanhGia);
}

public sealed class HangDoiPhanTichDanhGia(IOptions<AiServiceOptions> options) : IHangDoiPhanTichDanhGia
{
    private readonly Channel<int> _kenh = Channel.CreateBounded<int>(new BoundedChannelOptions(1_000)
    {
        SingleReader = true,
        FullMode = BoundedChannelFullMode.Wait // TryWrite trả false khi đầy, không chặn request
    });

    public ChannelReader<int> Reader => _kenh.Reader;

    public bool DuaVao(int maDanhGia) => options.Value.BatPhanTich && _kenh.Writer.TryWrite(maDanhGia);
}

/// <summary>
/// Đọc hàng đợi và gọi ai-service. ai-service không chạy → chỉ ghi log cảnh báo, không ảnh hưởng luồng duyệt.
/// Đánh giá bị bỏ lỡ có thể quét lại sau: DanhGia.TrangThai = 'DaDuyet' AND CamXuc IS NULL.
/// </summary>
public sealed class PhanTichDanhGiaWorker(
    HangDoiPhanTichDanhGia hangDoi,
    IServiceScopeFactory scopeFactory,
    ILogger<PhanTichDanhGiaWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var maDanhGia in hangDoi.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<IAiServiceClient>();
                await client.YeuCauPhanTichDanhGiaAsync(maDanhGia, stoppingToken);
                logger.LogInformation("Đã gửi DanhGia#{MaDanhGia} sang ai-service.", maDanhGia);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Không gửi được DanhGia#{MaDanhGia} sang ai-service: {Loi}", maDanhGia, ex.Message);
            }
        }
    }
}
