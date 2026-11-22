namespace ReviewIntelligence.Api.DTOs.TraCuu;

/// <summary>dbo.DanhMuc – trả danh sách phẳng; client dựng cây qua MaDanhMucCha.</summary>
public record DanhMucDto(int MaDanhMuc, int? MaDanhMucCha, string Ten, string DuongDan);

public record ThuongHieuDto(int MaThuongHieu, string Ten, string? QuocGia);

public record LoaiDaDto(int MaLoaiDa, string Ten);

public record KhiaCanhDto(int MaKhiaCanh, string Ten, string? MoTa);
