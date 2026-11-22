namespace ReviewIntelligence.Api.Services;

/// <summary>Kết quả nghiệp vụ: thành công kèm dữ liệu, hoặc lỗi kèm mã HTTP + thông báo.</summary>
public record KetQua<T>(bool OK, T? DuLieu, int MaLoi, string? ThongBao)
{
    public static KetQua<T> ThanhCong(T duLieu) => new(true, duLieu, 0, null);
    public static KetQua<T> Loi(int maLoi, string thongBao) => new(false, default, maLoi, thongBao);
}
