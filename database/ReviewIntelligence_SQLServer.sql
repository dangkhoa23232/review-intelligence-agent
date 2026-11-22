/* =====================================================================
   REVIEW INTELLIGENCE AGENT – WEBSITE MỸ PHẨM
   Mô hình vật lý dữ liệu – Microsoft SQL Server 2019+ (T-SQL)
   Gồm: tạo CSDL, 28 bảng, khóa, ràng buộc, chỉ mục (index), khung nhìn (view),
        trigger, thủ tục lưu trữ, mô tả cột (MS_Description) và dữ liệu mẫu.
   Vector nhúng (embedding) lưu ở Qdrant; SQL Server chỉ lưu mã điểm (DanhGia.MaVectorQdrant).
   Chạy toàn bộ tệp bằng SSMS / Azure Data Studio / sqlcmd.
   ===================================================================== */
SET NOCOUNT ON;
GO
IF DB_ID(N'ReviewIntelligenceDB') IS NULL
    CREATE DATABASE [ReviewIntelligenceDB] COLLATE Vietnamese_100_CI_AS;
GO
USE [ReviewIntelligenceDB];
GO

/* ---------- Xóa đối tượng cũ (thứ tự ngược phụ thuộc) ---------- */
DROP VIEW IF EXISTS dbo.vw_DoanhThuTheoNgay, dbo.vw_SanPhamBanChay, dbo.vw_TonKhoThap,
    dbo.vw_ThongKeKhiaCanh, dbo.vw_XuHuongCamXuc, dbo.vw_ChiSoThucNghiem;
GO
DROP PROCEDURE IF EXISTS dbo.sp_DoiTrangThaiDonHang;
GO
DROP TABLE IF EXISTS dbo.[KetQuaThucNghiem];
DROP TABLE IF EXISTS dbo.[MauKiemThu];
DROP TABLE IF EXISTS dbo.[NhatKyTacTu];
DROP TABLE IF EXISTS dbo.[TinNhan];
DROP TABLE IF EXISTS dbo.[TomTatKhiaCanh];
DROP TABLE IF EXISTS dbo.[KetQuaKhiaCanh];
DROP TABLE IF EXISTS dbo.[NhatKyHeThong];
DROP TABLE IF EXISTS dbo.[ThongBao];
DROP TABLE IF EXISTS dbo.[BaoCaoDanhGia];
DROP TABLE IF EXISTS dbo.[DiemTieuChi];
DROP TABLE IF EXISTS dbo.[DanhGia];
DROP TABLE IF EXISTS dbo.[KhiaCanh];
DROP TABLE IF EXISTS dbo.[ChiTietDonHang];
DROP TABLE IF EXISTS dbo.[DonHang];
DROP TABLE IF EXISTS dbo.[KhuyenMai];
DROP TABLE IF EXISTS dbo.[PhieuGiamGia];
DROP TABLE IF EXISTS dbo.[GioHang];
DROP TABLE IF EXISTS dbo.[YeuThich];
DROP TABLE IF EXISTS dbo.[ChiTietBoChamSoc];
DROP TABLE IF EXISTS dbo.[BoChamSoc];
DROP TABLE IF EXISTS dbo.[LoHang];
DROP TABLE IF EXISTS dbo.[BienThe];
DROP TABLE IF EXISTS dbo.[SanPham_LoaiDa];
DROP TABLE IF EXISTS dbo.[SanPham];
DROP TABLE IF EXISTS dbo.[ThuongHieu];
DROP TABLE IF EXISTS dbo.[DanhMuc];
DROP TABLE IF EXISTS dbo.[NguoiDung];
DROP TABLE IF EXISTS dbo.[LoaiDa];
GO


/* =================== Sản phẩm & tồn kho =================== */
-- Loại da
CREATE TABLE dbo.[LoaiDa] (
    [MaLoaiDa] INT IDENTITY(1,1) NOT NULL,
    [Ten] NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_LoaiDa PRIMARY KEY CLUSTERED ([MaLoaiDa]),
    CONSTRAINT UQ_LoaiDa_Ten UNIQUE ([Ten])
);
GO

/* =================== Tài khoản =================== */
-- Người dùng
CREATE TABLE dbo.[NguoiDung] (
    [MaNguoiDung] INT IDENTITY(1,1) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [MatKhauBam] VARCHAR(255) NOT NULL,
    [HoTen] NVARCHAR(150) NOT NULL,
    [SoDienThoai] VARCHAR(20) NULL,
    [DiaChi] NVARCHAR(300) NULL,
    [MaLoaiDa] INT NULL,
    [VaiTro] VARCHAR(10) NOT NULL CONSTRAINT DF_NguoiDung_VaiTro DEFAULT ('KhachHang'),
    [MaDatLaiMatKhau] VARCHAR(255) NULL,
    [HanMaDatLai] DATETIME2(0) NULL,
    [DangHoatDong] BIT NOT NULL CONSTRAINT DF_NguoiDung_DangHoatDong DEFAULT (1),
    [NgayTao] DATETIME2(0) NOT NULL CONSTRAINT DF_NguoiDung_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_NguoiDung PRIMARY KEY CLUSTERED ([MaNguoiDung]),
    CONSTRAINT UQ_NguoiDung_Email UNIQUE ([Email]),
    CONSTRAINT CK_NguoiDung_VaiTro CHECK (VaiTro IN ('KhachHang','QuanTri')),
    CONSTRAINT FK_NguoiDung_MaLoaiDa FOREIGN KEY ([MaLoaiDa]) REFERENCES dbo.[LoaiDa]([MaLoaiDa])
);
GO

/* =================== Sản phẩm & tồn kho =================== */
-- Danh mục
CREATE TABLE dbo.[DanhMuc] (
    [MaDanhMuc] INT IDENTITY(1,1) NOT NULL,
    [MaDanhMucCha] INT NULL,
    [Ten] NVARCHAR(150) NOT NULL,
    [DuongDan] VARCHAR(160) NOT NULL,
    CONSTRAINT PK_DanhMuc PRIMARY KEY CLUSTERED ([MaDanhMuc]),
    CONSTRAINT UQ_DanhMuc_DuongDan UNIQUE ([DuongDan]),
    CONSTRAINT FK_DanhMuc_MaDanhMucCha FOREIGN KEY ([MaDanhMucCha]) REFERENCES dbo.[DanhMuc]([MaDanhMuc])
);
GO
-- Thương hiệu
CREATE TABLE dbo.[ThuongHieu] (
    [MaThuongHieu] INT IDENTITY(1,1) NOT NULL,
    [Ten] NVARCHAR(150) NOT NULL,
    [QuocGia] NVARCHAR(100) NULL,
    CONSTRAINT PK_ThuongHieu PRIMARY KEY CLUSTERED ([MaThuongHieu]),
    CONSTRAINT UQ_ThuongHieu_Ten UNIQUE ([Ten])
);
GO
-- Sản phẩm
CREATE TABLE dbo.[SanPham] (
    [MaSanPham] INT IDENTITY(1,1) NOT NULL,
    [MaDanhMuc] INT NOT NULL,
    [MaThuongHieu] INT NOT NULL,
    [Ten] NVARCHAR(255) NOT NULL,
    [DuongDan] VARCHAR(280) NOT NULL,
    [MoTa] NVARCHAR(MAX) NULL,
    [CongDung] NVARCHAR(300) NULL,
    [ThanhPhan] NVARCHAR(MAX) NULL,
    [HuongDanSuDung] NVARCHAR(1000) NULL,
    [DanhSachAnh] NVARCHAR(MAX) NULL,
    [TrangThai] VARCHAR(10) NOT NULL CONSTRAINT DF_SanPham_TrangThai DEFAULT ('DangBan'),
    [DiemTrungBinh] DECIMAL(3,2) NOT NULL CONSTRAINT DF_SanPham_DiemTrungBinh DEFAULT (0),
    [SoDanhGia] INT NOT NULL CONSTRAINT DF_SanPham_SoDanhGia DEFAULT (0),
    [NgayTao] DATETIME2(0) NOT NULL CONSTRAINT DF_SanPham_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_SanPham PRIMARY KEY CLUSTERED ([MaSanPham]),
    CONSTRAINT UQ_SanPham_DuongDan UNIQUE ([DuongDan]),
    CONSTRAINT CK_SanPham_DanhSachAnh CHECK (DanhSachAnh IS NULL OR ISJSON(DanhSachAnh) = 1),
    CONSTRAINT CK_SanPham_TrangThai CHECK (TrangThai IN ('DangBan','NgungBan')),
    CONSTRAINT FK_SanPham_MaDanhMuc FOREIGN KEY ([MaDanhMuc]) REFERENCES dbo.[DanhMuc]([MaDanhMuc]),
    CONSTRAINT FK_SanPham_MaThuongHieu FOREIGN KEY ([MaThuongHieu]) REFERENCES dbo.[ThuongHieu]([MaThuongHieu])
);
GO
CREATE NONCLUSTERED INDEX IX_SanPham_MaDanhMuc ON dbo.[SanPham] ([MaDanhMuc]);
GO
CREATE NONCLUSTERED INDEX IX_SanPham_MaThuongHieu ON dbo.[SanPham] ([MaThuongHieu]);
GO
CREATE NONCLUSTERED INDEX IX_SanPham_Ten ON dbo.[SanPham] ([Ten]);
GO
-- Sản phẩm – loại da phù hợp
CREATE TABLE dbo.[SanPham_LoaiDa] (
    [MaSanPham] INT NOT NULL,
    [MaLoaiDa] INT NOT NULL,
    CONSTRAINT PK_SanPham_LoaiDa PRIMARY KEY CLUSTERED ([MaSanPham], [MaLoaiDa]),
    CONSTRAINT FK_SanPham_LoaiDa_MaSanPham FOREIGN KEY ([MaSanPham]) REFERENCES dbo.[SanPham]([MaSanPham]) ON DELETE CASCADE,
    CONSTRAINT FK_SanPham_LoaiDa_MaLoaiDa FOREIGN KEY ([MaLoaiDa]) REFERENCES dbo.[LoaiDa]([MaLoaiDa]) ON DELETE CASCADE
);
GO
-- Biến thể sản phẩm (SKU)
CREATE TABLE dbo.[BienThe] (
    [MaBienThe] INT IDENTITY(1,1) NOT NULL,
    [MaSanPham] INT NOT NULL,
    [MaSKU] VARCHAR(50) NOT NULL,
    [DungTich] NVARCHAR(30) NOT NULL,
    [GiaBan] DECIMAL(18,2) NOT NULL,
    [GiaKhuyenMai] DECIMAL(18,2) NULL,
    [KhuyenMaiTu] DATETIME2(0) NULL,
    [KhuyenMaiDen] DATETIME2(0) NULL,
    [SoLuongTon] INT NOT NULL CONSTRAINT DF_BienThe_SoLuongTon DEFAULT (0),
    [DangHoatDong] BIT NOT NULL CONSTRAINT DF_BienThe_DangHoatDong DEFAULT (1),
    CONSTRAINT PK_BienThe PRIMARY KEY CLUSTERED ([MaBienThe]),
    CONSTRAINT UQ_BienThe_MaSKU UNIQUE ([MaSKU]),
    CONSTRAINT CK_BienThe_GiaBan CHECK (GiaBan >= 0),
    CONSTRAINT CK_BienThe_SoLuongTon CHECK (SoLuongTon >= 0),
    CONSTRAINT CK_BienThe_1 CHECK (GiaKhuyenMai IS NULL OR (GiaKhuyenMai < GiaBan AND KhuyenMaiDen > KhuyenMaiTu)),
    CONSTRAINT FK_BienThe_MaSanPham FOREIGN KEY ([MaSanPham]) REFERENCES dbo.[SanPham]([MaSanPham])
);
GO
CREATE NONCLUSTERED INDEX IX_BienThe_MaSanPham ON dbo.[BienThe] ([MaSanPham]);
GO
-- Lô hàng & hạn sử dụng
CREATE TABLE dbo.[LoHang] (
    [MaLo] INT IDENTITY(1,1) NOT NULL,
    [MaBienThe] INT NOT NULL,
    [SoLo] VARCHAR(50) NOT NULL,
    [HanSuDung] DATE NOT NULL,
    [SoLuongNhap] INT NOT NULL,
    [SoLuongConLai] INT NOT NULL,
    [NgayNhap] DATETIME2(0) NOT NULL CONSTRAINT DF_LoHang_NgayNhap DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_LoHang PRIMARY KEY CLUSTERED ([MaLo]),
    CONSTRAINT UQ_LoHang_MaBienThe_SoLo UNIQUE ([MaBienThe], [SoLo]),
    CONSTRAINT CK_LoHang_SoLuongNhap CHECK (SoLuongNhap > 0),
    CONSTRAINT CK_LoHang_SoLuongConLai CHECK (SoLuongConLai >= 0),
    CONSTRAINT CK_LoHang_1 CHECK (SoLuongConLai <= SoLuongNhap),
    CONSTRAINT FK_LoHang_MaBienThe FOREIGN KEY ([MaBienThe]) REFERENCES dbo.[BienThe]([MaBienThe])
);
GO
CREATE NONCLUSTERED INDEX IX_LoHang_MaBienThe_HanSuDung ON dbo.[LoHang] ([MaBienThe], [HanSuDung]);
GO
-- Bộ chăm sóc da (routine bundle)
CREATE TABLE dbo.[BoChamSoc] (
    [MaBo] INT IDENTITY(1,1) NOT NULL,
    [Ten] NVARCHAR(200) NOT NULL,
    [MoTa] NVARCHAR(1000) NULL,
    [MaLoaiDa] INT NULL,
    [PhanTramGiam] DECIMAL(5,2) NOT NULL CONSTRAINT DF_BoChamSoc_PhanTramGiam DEFAULT (0),
    CONSTRAINT PK_BoChamSoc PRIMARY KEY CLUSTERED ([MaBo]),
    CONSTRAINT CK_BoChamSoc_PhanTramGiam CHECK (PhanTramGiam BETWEEN 0 AND 100),
    CONSTRAINT FK_BoChamSoc_MaLoaiDa FOREIGN KEY ([MaLoaiDa]) REFERENCES dbo.[LoaiDa]([MaLoaiDa])
);
GO
-- Các bước trong bộ chăm sóc
CREATE TABLE dbo.[ChiTietBoChamSoc] (
    [MaBo] INT NOT NULL,
    [MaBienThe] INT NOT NULL,
    [ThuTuBuoc] TINYINT NOT NULL,
    [TenBuoc] NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_ChiTietBoChamSoc PRIMARY KEY CLUSTERED ([MaBo], [MaBienThe]),
    CONSTRAINT FK_ChiTietBoChamSoc_MaBo FOREIGN KEY ([MaBo]) REFERENCES dbo.[BoChamSoc]([MaBo]) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietBoChamSoc_MaBienThe FOREIGN KEY ([MaBienThe]) REFERENCES dbo.[BienThe]([MaBienThe])
);
GO
-- Danh sách yêu thích
CREATE TABLE dbo.[YeuThich] (
    [MaNguoiDung] INT NOT NULL,
    [MaSanPham] INT NOT NULL,
    [NgayTao] DATETIME2(0) NOT NULL CONSTRAINT DF_YeuThich_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_YeuThich PRIMARY KEY CLUSTERED ([MaNguoiDung], [MaSanPham]),
    CONSTRAINT FK_YeuThich_MaNguoiDung FOREIGN KEY ([MaNguoiDung]) REFERENCES dbo.[NguoiDung]([MaNguoiDung]) ON DELETE CASCADE,
    CONSTRAINT FK_YeuThich_MaSanPham FOREIGN KEY ([MaSanPham]) REFERENCES dbo.[SanPham]([MaSanPham]) ON DELETE CASCADE
);
GO

/* =================== Giỏ hàng, đơn hàng & khuyến mãi =================== */
-- Giỏ hàng
CREATE TABLE dbo.[GioHang] (
    [MaNguoiDung] INT NOT NULL,
    [MaBienThe] INT NOT NULL,
    [SoLuong] INT NOT NULL,
    [NgayThem] DATETIME2(0) NOT NULL CONSTRAINT DF_GioHang_NgayThem DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_GioHang PRIMARY KEY CLUSTERED ([MaNguoiDung], [MaBienThe]),
    CONSTRAINT CK_GioHang_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT FK_GioHang_MaNguoiDung FOREIGN KEY ([MaNguoiDung]) REFERENCES dbo.[NguoiDung]([MaNguoiDung]) ON DELETE CASCADE,
    CONSTRAINT FK_GioHang_MaBienThe FOREIGN KEY ([MaBienThe]) REFERENCES dbo.[BienThe]([MaBienThe]) ON DELETE CASCADE
);
GO
-- Phiếu giảm giá (voucher)
CREATE TABLE dbo.[PhieuGiamGia] (
    [MaPhieu] INT IDENTITY(1,1) NOT NULL,
    [MaCode] VARCHAR(30) NOT NULL,
    [KieuGiam] VARCHAR(10) NOT NULL,
    [GiaTriGiam] DECIMAL(18,2) NOT NULL,
    [GiamToiDa] DECIMAL(18,2) NULL,
    [DonToiThieu] DECIMAL(18,2) NOT NULL CONSTRAINT DF_PhieuGiamGia_DonToiThieu DEFAULT (0),
    [SoLuotConLai] INT NULL,
    [BatDau] DATETIME2(0) NOT NULL,
    [KetThuc] DATETIME2(0) NOT NULL,
    CONSTRAINT PK_PhieuGiamGia PRIMARY KEY CLUSTERED ([MaPhieu]),
    CONSTRAINT UQ_PhieuGiamGia_MaCode UNIQUE ([MaCode]),
    CONSTRAINT CK_PhieuGiamGia_KieuGiam CHECK (KieuGiam IN ('PhanTram','SoTien')),
    CONSTRAINT CK_PhieuGiamGia_GiaTriGiam CHECK (GiaTriGiam > 0),
    CONSTRAINT CK_PhieuGiamGia_1 CHECK (KetThuc > BatDau)
);
GO
-- Khuyến mãi theo sản phẩm / danh mục
CREATE TABLE dbo.[KhuyenMai] (
    [MaKhuyenMai] INT IDENTITY(1,1) NOT NULL,
    [Ten] NVARCHAR(200) NOT NULL,
    [PhanTramGiam] DECIMAL(5,2) NOT NULL,
    [MaSanPham] INT NULL,
    [MaDanhMuc] INT NULL,
    [BatDau] DATETIME2(0) NOT NULL,
    [KetThuc] DATETIME2(0) NOT NULL,
    CONSTRAINT PK_KhuyenMai PRIMARY KEY CLUSTERED ([MaKhuyenMai]),
    CONSTRAINT CK_KhuyenMai_PhanTramGiam CHECK (PhanTramGiam BETWEEN 0 AND 100),
    CONSTRAINT CK_KhuyenMai_1 CHECK (KetThuc > BatDau),
    CONSTRAINT CK_KhuyenMai_2 CHECK ((MaSanPham IS NOT NULL AND MaDanhMuc IS NULL) OR (MaSanPham IS NULL AND MaDanhMuc IS NOT NULL)),
    CONSTRAINT FK_KhuyenMai_MaSanPham FOREIGN KEY ([MaSanPham]) REFERENCES dbo.[SanPham]([MaSanPham]),
    CONSTRAINT FK_KhuyenMai_MaDanhMuc FOREIGN KEY ([MaDanhMuc]) REFERENCES dbo.[DanhMuc]([MaDanhMuc])
);
GO
-- Đơn hàng
CREATE TABLE dbo.[DonHang] (
    [MaDonHang] INT IDENTITY(1,1) NOT NULL,
    [MaNguoiDung] INT NOT NULL,
    [MaPhieu] INT NULL,
    [TenNguoiNhan] NVARCHAR(150) NOT NULL,
    [SoDienThoai] VARCHAR(20) NOT NULL,
    [DiaChiGiaoHang] NVARCHAR(300) NOT NULL,
    [PhuongThucGiaoHang] VARCHAR(10) NOT NULL,
    [PhuongThucThanhToan] VARCHAR(20) NOT NULL,
    [TienHang] DECIMAL(18,2) NOT NULL,
    [TienGiam] DECIMAL(18,2) NOT NULL CONSTRAINT DF_DonHang_TienGiam DEFAULT (0),
    [PhiVanChuyen] DECIMAL(18,2) NOT NULL CONSTRAINT DF_DonHang_PhiVanChuyen DEFAULT (0),
    [TongTien] DECIMAL(18,2) NOT NULL,
    [TrangThai] VARCHAR(20) NOT NULL CONSTRAINT DF_DonHang_TrangThai DEFAULT ('ChoXacNhan'),
    [DaThanhToan] BIT NOT NULL CONSTRAINT DF_DonHang_DaThanhToan DEFAULT (0),
    [NgayDat] DATETIME2(0) NOT NULL CONSTRAINT DF_DonHang_NgayDat DEFAULT (SYSUTCDATETIME()),
    [NgayCapNhat] DATETIME2(0) NULL,
    CONSTRAINT PK_DonHang PRIMARY KEY CLUSTERED ([MaDonHang]),
    CONSTRAINT CK_DonHang_PhuongThucGiaoHang CHECK (PhuongThucGiaoHang IN ('TieuChuan','Nhanh')),
    CONSTRAINT CK_DonHang_PhuongThucThanhToan CHECK (PhuongThucThanhToan IN ('TienMat','ChuyenKhoanGiaLap')),
    CONSTRAINT CK_DonHang_TrangThai CHECK (TrangThai IN ('ChoXacNhan','DaXacNhan','DangDongGoi','DangGiao','HoanTat','DaHuy')),
    CONSTRAINT CK_DonHang_1 CHECK (TongTien = TienHang - TienGiam + PhiVanChuyen AND TongTien >= 0),
    CONSTRAINT FK_DonHang_MaNguoiDung FOREIGN KEY ([MaNguoiDung]) REFERENCES dbo.[NguoiDung]([MaNguoiDung]),
    CONSTRAINT FK_DonHang_MaPhieu FOREIGN KEY ([MaPhieu]) REFERENCES dbo.[PhieuGiamGia]([MaPhieu])
);
GO
CREATE NONCLUSTERED INDEX IX_DonHang_MaNguoiDung_NgayDat ON dbo.[DonHang] ([MaNguoiDung], [NgayDat] DESC);
GO
CREATE NONCLUSTERED INDEX IX_DonHang_TrangThai ON dbo.[DonHang] ([TrangThai]);
GO
-- Chi tiết đơn hàng
CREATE TABLE dbo.[ChiTietDonHang] (
    [MaChiTietDH] INT IDENTITY(1,1) NOT NULL,
    [MaDonHang] INT NOT NULL,
    [MaBienThe] INT NOT NULL,
    [MaLo] INT NULL,
    [DonGia] DECIMAL(18,2) NOT NULL,
    [SoLuong] INT NOT NULL,
    [ThanhTien] DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_ChiTietDonHang PRIMARY KEY CLUSTERED ([MaChiTietDH]),
    CONSTRAINT CK_ChiTietDonHang_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT FK_ChiTietDonHang_MaDonHang FOREIGN KEY ([MaDonHang]) REFERENCES dbo.[DonHang]([MaDonHang]) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietDonHang_MaBienThe FOREIGN KEY ([MaBienThe]) REFERENCES dbo.[BienThe]([MaBienThe]),
    CONSTRAINT FK_ChiTietDonHang_MaLo FOREIGN KEY ([MaLo]) REFERENCES dbo.[LoHang]([MaLo])
);
GO
CREATE NONCLUSTERED INDEX IX_ChiTietDonHang_MaDonHang ON dbo.[ChiTietDonHang] ([MaDonHang]);
GO

/* =================== Đánh giá =================== */
-- Khía cạnh / tiêu chí đánh giá
CREATE TABLE dbo.[KhiaCanh] (
    [MaKhiaCanh] INT IDENTITY(1,1) NOT NULL,
    [Ten] NVARCHAR(50) NOT NULL,
    [MoTa] NVARCHAR(300) NULL,
    CONSTRAINT PK_KhiaCanh PRIMARY KEY CLUSTERED ([MaKhiaCanh]),
    CONSTRAINT UQ_KhiaCanh_Ten UNIQUE ([Ten])
);
GO
-- Đánh giá sản phẩm
CREATE TABLE dbo.[DanhGia] (
    [MaDanhGia] INT IDENTITY(1,1) NOT NULL,
    [MaSanPham] INT NOT NULL,
    [MaNguoiDung] INT NOT NULL,
    [MaChiTietDH] INT NULL,
    [SoSao] TINYINT NOT NULL,
    [NoiDung] NVARCHAR(2000) NOT NULL,
    [DaMuaHang] BIT NOT NULL CONSTRAINT DF_DanhGia_DaMuaHang DEFAULT (0),
    [TrangThai] VARCHAR(10) NOT NULL CONSTRAINT DF_DanhGia_TrangThai DEFAULT ('ChoDuyet'),
    [CamXuc] VARCHAR(10) NULL,
    [DoTinCay] DECIMAL(5,4) NULL,
    [MaVectorQdrant] UNIQUEIDENTIFIER NULL,
    [NgayTao] DATETIME2(0) NOT NULL CONSTRAINT DF_DanhGia_NgayTao DEFAULT (SYSUTCDATETIME()),
    [NgayCapNhat] DATETIME2(0) NULL,
    CONSTRAINT PK_DanhGia PRIMARY KEY CLUSTERED ([MaDanhGia]),
    CONSTRAINT CK_DanhGia_SoSao CHECK (SoSao BETWEEN 1 AND 5),
    CONSTRAINT CK_DanhGia_TrangThai CHECK (TrangThai IN ('ChoDuyet','DaDuyet','DaAn')),
    CONSTRAINT CK_DanhGia_CamXuc CHECK (CamXuc IN ('TichCuc','TrungTinh','TieuCuc')),
    CONSTRAINT CK_DanhGia_DoTinCay CHECK (DoTinCay BETWEEN 0 AND 1),
    CONSTRAINT FK_DanhGia_MaSanPham FOREIGN KEY ([MaSanPham]) REFERENCES dbo.[SanPham]([MaSanPham]),
    CONSTRAINT FK_DanhGia_MaNguoiDung FOREIGN KEY ([MaNguoiDung]) REFERENCES dbo.[NguoiDung]([MaNguoiDung]),
    CONSTRAINT FK_DanhGia_MaChiTietDH FOREIGN KEY ([MaChiTietDH]) REFERENCES dbo.[ChiTietDonHang]([MaChiTietDH])
);
GO
CREATE NONCLUSTERED INDEX IX_DanhGia_MaSanPham_TrangThai ON dbo.[DanhGia] ([MaSanPham], [TrangThai]);
GO
CREATE NONCLUSTERED INDEX IX_DanhGia_MaNguoiDung ON dbo.[DanhGia] ([MaNguoiDung]);
GO
-- Điểm theo tiêu chí
CREATE TABLE dbo.[DiemTieuChi] (
    [MaDanhGia] INT NOT NULL,
    [MaKhiaCanh] INT NOT NULL,
    [Diem] TINYINT NOT NULL,
    CONSTRAINT PK_DiemTieuChi PRIMARY KEY CLUSTERED ([MaDanhGia], [MaKhiaCanh]),
    CONSTRAINT CK_DiemTieuChi_Diem CHECK (Diem BETWEEN 1 AND 5),
    CONSTRAINT FK_DiemTieuChi_MaDanhGia FOREIGN KEY ([MaDanhGia]) REFERENCES dbo.[DanhGia]([MaDanhGia]) ON DELETE CASCADE,
    CONSTRAINT FK_DiemTieuChi_MaKhiaCanh FOREIGN KEY ([MaKhiaCanh]) REFERENCES dbo.[KhiaCanh]([MaKhiaCanh])
);
GO
-- Báo cáo đánh giá không phù hợp
CREATE TABLE dbo.[BaoCaoDanhGia] (
    [MaBaoCao] INT IDENTITY(1,1) NOT NULL,
    [MaDanhGia] INT NOT NULL,
    [MaNguoiBaoCao] INT NOT NULL,
    [LyDo] NVARCHAR(300) NOT NULL,
    [TrangThai] VARCHAR(10) NOT NULL CONSTRAINT DF_BaoCaoDanhGia_TrangThai DEFAULT ('ChoXuLy'),
    [NgayTao] DATETIME2(0) NOT NULL CONSTRAINT DF_BaoCaoDanhGia_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_BaoCaoDanhGia PRIMARY KEY CLUSTERED ([MaBaoCao]),
    CONSTRAINT UQ_BaoCaoDanhGia_MaDanhGia_MaNguoiBaoCao UNIQUE ([MaDanhGia], [MaNguoiBaoCao]),
    CONSTRAINT CK_BaoCaoDanhGia_TrangThai CHECK (TrangThai IN ('ChoXuLy','DaXuLy','BoQua')),
    CONSTRAINT FK_BaoCaoDanhGia_MaDanhGia FOREIGN KEY ([MaDanhGia]) REFERENCES dbo.[DanhGia]([MaDanhGia]) ON DELETE CASCADE,
    CONSTRAINT FK_BaoCaoDanhGia_MaNguoiBaoCao FOREIGN KEY ([MaNguoiBaoCao]) REFERENCES dbo.[NguoiDung]([MaNguoiDung])
);
GO

/* =================== Thông báo & nhật ký =================== */
-- Thông báo
CREATE TABLE dbo.[ThongBao] (
    [MaThongBao] BIGINT IDENTITY(1,1) NOT NULL,
    [MaNguoiDung] INT NOT NULL,
    [NoiDung] NVARCHAR(500) NOT NULL,
    [LienKet] NVARCHAR(300) NULL,
    [DaDoc] BIT NOT NULL CONSTRAINT DF_ThongBao_DaDoc DEFAULT (0),
    [NgayTao] DATETIME2(0) NOT NULL CONSTRAINT DF_ThongBao_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ThongBao PRIMARY KEY CLUSTERED ([MaThongBao]),
    CONSTRAINT FK_ThongBao_MaNguoiDung FOREIGN KEY ([MaNguoiDung]) REFERENCES dbo.[NguoiDung]([MaNguoiDung]) ON DELETE CASCADE
);
GO
CREATE NONCLUSTERED INDEX IX_ThongBao_MaNguoiDung_DaDoc ON dbo.[ThongBao] ([MaNguoiDung], [DaDoc]);
GO
-- Nhật ký thao tác (audit log)
CREATE TABLE dbo.[NhatKyHeThong] (
    [MaNhatKy] BIGINT IDENTITY(1,1) NOT NULL,
    [MaNguoiDung] INT NULL,
    [HanhDong] VARCHAR(50) NOT NULL,
    [DoiTuong] VARCHAR(100) NOT NULL,
    [ChiTiet] NVARCHAR(MAX) NULL,
    [NgayTao] DATETIME2(0) NOT NULL CONSTRAINT DF_NhatKyHeThong_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_NhatKyHeThong PRIMARY KEY CLUSTERED ([MaNhatKy]),
    CONSTRAINT CK_NhatKyHeThong_ChiTiet CHECK (ChiTiet IS NULL OR ISJSON(ChiTiet) = 1),
    CONSTRAINT FK_NhatKyHeThong_MaNguoiDung FOREIGN KEY ([MaNguoiDung]) REFERENCES dbo.[NguoiDung]([MaNguoiDung])
);
GO
CREATE NONCLUSTERED INDEX IX_NhatKyHeThong_NgayTao ON dbo.[NhatKyHeThong] ([NgayTao] DESC);
GO

/* =================== Phân tích AI, tác tử & thực nghiệm =================== */
-- Kết quả cảm xúc theo khía cạnh (ABSA)
CREATE TABLE dbo.[KetQuaKhiaCanh] (
    [MaKetQua] BIGINT IDENTITY(1,1) NOT NULL,
    [MaDanhGia] INT NOT NULL,
    [MaKhiaCanh] INT NOT NULL,
    [CamXuc] VARCHAR(10) NOT NULL,
    [CumTuDanChung] NVARCHAR(300) NULL,
    [MoHinh] VARCHAR(20) NOT NULL,
    [NgayTao] DATETIME2(0) NOT NULL CONSTRAINT DF_KetQuaKhiaCanh_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_KetQuaKhiaCanh PRIMARY KEY CLUSTERED ([MaKetQua]),
    CONSTRAINT UQ_KetQuaKhiaCanh_MaDanhGia_MaKhiaCanh UNIQUE ([MaDanhGia], [MaKhiaCanh]),
    CONSTRAINT CK_KetQuaKhiaCanh_CamXuc CHECK (CamXuc IN ('TichCuc','TrungTinh','TieuCuc')),
    CONSTRAINT CK_KetQuaKhiaCanh_MoHinh CHECK (MoHinh IN ('PhoBERT','LLM_KhongVD','LLM_ItVD')),
    CONSTRAINT FK_KetQuaKhiaCanh_MaDanhGia FOREIGN KEY ([MaDanhGia]) REFERENCES dbo.[DanhGia]([MaDanhGia]) ON DELETE CASCADE,
    CONSTRAINT FK_KetQuaKhiaCanh_MaKhiaCanh FOREIGN KEY ([MaKhiaCanh]) REFERENCES dbo.[KhiaCanh]([MaKhiaCanh])
);
GO
CREATE NONCLUSTERED INDEX IX_KetQuaKhiaCanh_MaKhiaCanh_CamXuc ON dbo.[KetQuaKhiaCanh] ([MaKhiaCanh], [CamXuc]);
GO
-- Tóm tắt ưu/nhược theo khía cạnh
CREATE TABLE dbo.[TomTatKhiaCanh] (
    [MaTomTat] INT IDENTITY(1,1) NOT NULL,
    [MaSanPham] INT NOT NULL,
    [MaKhiaCanh] INT NOT NULL,
    [SoTichCuc] INT NOT NULL CONSTRAINT DF_TomTatKhiaCanh_SoTichCuc DEFAULT (0),
    [SoTieuCuc] INT NOT NULL CONSTRAINT DF_TomTatKhiaCanh_SoTieuCuc DEFAULT (0),
    [UuDiem] NVARCHAR(1000) NULL,
    [NhuocDiem] NVARCHAR(1000) NULL,
    [VanDeLapLai] NVARCHAR(500) NULL,
    [DanChung] NVARCHAR(MAX) NULL,
    [NgayCapNhat] DATETIME2(0) NOT NULL CONSTRAINT DF_TomTatKhiaCanh_NgayCapNhat DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_TomTatKhiaCanh PRIMARY KEY CLUSTERED ([MaTomTat]),
    CONSTRAINT UQ_TomTatKhiaCanh_MaSanPham_MaKhiaCanh UNIQUE ([MaSanPham], [MaKhiaCanh]),
    CONSTRAINT CK_TomTatKhiaCanh_DanChung CHECK (DanChung IS NULL OR ISJSON(DanChung) = 1),
    CONSTRAINT FK_TomTatKhiaCanh_MaSanPham FOREIGN KEY ([MaSanPham]) REFERENCES dbo.[SanPham]([MaSanPham]) ON DELETE CASCADE,
    CONSTRAINT FK_TomTatKhiaCanh_MaKhiaCanh FOREIGN KEY ([MaKhiaCanh]) REFERENCES dbo.[KhiaCanh]([MaKhiaCanh])
);
GO
-- Tin nhắn hỏi đáp với tác tử
CREATE TABLE dbo.[TinNhan] (
    [MaTinNhan] BIGINT IDENTITY(1,1) NOT NULL,
    [MaPhien] UNIQUEIDENTIFIER NOT NULL,
    [MaNguoiDung] INT NULL,
    [MaSanPham] INT NULL,
    [NguoiGui] VARCHAR(10) NOT NULL,
    [NoiDung] NVARCHAR(MAX) NOT NULL,
    [DanChung] NVARCHAR(MAX) NULL,
    [DoTreMs] INT NULL,
    [NgayTao] DATETIME2(0) NOT NULL CONSTRAINT DF_TinNhan_NgayTao DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_TinNhan PRIMARY KEY CLUSTERED ([MaTinNhan]),
    CONSTRAINT CK_TinNhan_NguoiGui CHECK (NguoiGui IN ('NguoiDung','TroLy')),
    CONSTRAINT CK_TinNhan_DanChung CHECK (DanChung IS NULL OR ISJSON(DanChung) = 1),
    CONSTRAINT FK_TinNhan_MaNguoiDung FOREIGN KEY ([MaNguoiDung]) REFERENCES dbo.[NguoiDung]([MaNguoiDung]),
    CONSTRAINT FK_TinNhan_MaSanPham FOREIGN KEY ([MaSanPham]) REFERENCES dbo.[SanPham]([MaSanPham])
);
GO
CREATE NONCLUSTERED INDEX IX_TinNhan_MaPhien_NgayTao ON dbo.[TinNhan] ([MaPhien], [NgayTao]);
GO
-- Nhật ký các bước của tác tử (trace)
CREATE TABLE dbo.[NhatKyTacTu] (
    [MaBuoc] BIGINT IDENTITY(1,1) NOT NULL,
    [MaTinNhan] BIGINT NOT NULL,
    [BuocSo] INT NOT NULL,
    [LoaiBuoc] VARCHAR(20) NOT NULL,
    [TenCongCu] VARCHAR(50) NULL,
    [DauVao] NVARCHAR(MAX) NULL,
    [DauRa] NVARCHAR(MAX) NULL,
    [DoTreMs] INT NULL,
    CONSTRAINT PK_NhatKyTacTu PRIMARY KEY CLUSTERED ([MaBuoc]),
    CONSTRAINT UQ_NhatKyTacTu_MaTinNhan_BuocSo UNIQUE ([MaTinNhan], [BuocSo]),
    CONSTRAINT CK_NhatKyTacTu_LoaiBuoc CHECK (LoaiBuoc IN ('LapKeHoach','GoiCongCu','QuanSat','KiemSoat','TraLoi','Loi')),
    CONSTRAINT CK_NhatKyTacTu_DauVao CHECK (DauVao IS NULL OR ISJSON(DauVao) = 1),
    CONSTRAINT CK_NhatKyTacTu_DauRa CHECK (DauRa IS NULL OR ISJSON(DauRa) = 1),
    CONSTRAINT FK_NhatKyTacTu_MaTinNhan FOREIGN KEY ([MaTinNhan]) REFERENCES dbo.[TinNhan]([MaTinNhan]) ON DELETE CASCADE
);
GO
-- Mẫu trong bộ kiểm thử (test set)
CREATE TABLE dbo.[MauKiemThu] (
    [MaMau] INT IDENTITY(1,1) NOT NULL,
    [MaDanhGia] INT NULL,
    [NoiDung] NVARCHAR(2000) NOT NULL,
    [CamXucChuan] VARCHAR(10) NOT NULL,
    [KhiaCanhChuan] NVARCHAR(MAX) NULL,
    [TapDuLieu] VARCHAR(10) NOT NULL CONSTRAINT DF_MauKiemThu_TapDuLieu DEFAULT ('KiemThu'),
    CONSTRAINT PK_MauKiemThu PRIMARY KEY CLUSTERED ([MaMau]),
    CONSTRAINT CK_MauKiemThu_CamXucChuan CHECK (CamXucChuan IN ('TichCuc','TrungTinh','TieuCuc')),
    CONSTRAINT CK_MauKiemThu_KhiaCanhChuan CHECK (KhiaCanhChuan IS NULL OR ISJSON(KhiaCanhChuan) = 1),
    CONSTRAINT CK_MauKiemThu_TapDuLieu CHECK (TapDuLieu IN ('KiemThu','ViDuMau')),
    CONSTRAINT FK_MauKiemThu_MaDanhGia FOREIGN KEY ([MaDanhGia]) REFERENCES dbo.[DanhGia]([MaDanhGia])
);
GO
-- Kết quả thực nghiệm từng mẫu
CREATE TABLE dbo.[KetQuaThucNghiem] (
    [MaKetQua] BIGINT IDENTITY(1,1) NOT NULL,
    [MaMau] INT NOT NULL,
    [MoHinh] VARCHAR(20) NOT NULL,
    [CamXucDuDoan] VARCHAR(10) NOT NULL,
    [KhiaCanhDuDoan] NVARCHAR(MAX) NULL,
    [LaDung] BIT NOT NULL,
    [NhomLoi] NVARCHAR(100) NULL,
    [DoTreMs] INT NULL,
    [NgayChay] DATETIME2(0) NOT NULL CONSTRAINT DF_KetQuaThucNghiem_NgayChay DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_KetQuaThucNghiem PRIMARY KEY CLUSTERED ([MaKetQua]),
    CONSTRAINT UQ_KetQuaThucNghiem_MaMau_MoHinh UNIQUE ([MaMau], [MoHinh]),
    CONSTRAINT CK_KetQuaThucNghiem_MoHinh CHECK (MoHinh IN ('PhoBERT','LLM_KhongVD','LLM_ItVD')),
    CONSTRAINT CK_KetQuaThucNghiem_CamXucDuDoan CHECK (CamXucDuDoan IN ('TichCuc','TrungTinh','TieuCuc')),
    CONSTRAINT CK_KetQuaThucNghiem_KhiaCanhDuDoan CHECK (KhiaCanhDuDoan IS NULL OR ISJSON(KhiaCanhDuDoan) = 1),
    CONSTRAINT FK_KetQuaThucNghiem_MaMau FOREIGN KEY ([MaMau]) REFERENCES dbo.[MauKiemThu]([MaMau]) ON DELETE CASCADE
);
GO

/* =================== Chỉ mục có lọc, khung nhìn, trigger, thủ tục =================== */

-- Mỗi dòng đơn hàng chỉ được đánh giá 1 lần (xác thực đã mua hàng)
CREATE UNIQUE NONCLUSTERED INDEX UX_DanhGia_MaChiTietDH
    ON dbo.DanhGia(MaChiTietDH) WHERE MaChiTietDH IS NOT NULL;
GO

/* ---------- Khung nhìn cho bảng điều khiển ---------- */
CREATE VIEW dbo.vw_DoanhThuTheoNgay AS
SELECT CAST(NgayDat AS DATE) AS Ngay, COUNT(*) AS SoDonHang, SUM(TongTien) AS DoanhThu
FROM dbo.DonHang
WHERE TrangThai = 'HoanTat'
GROUP BY CAST(NgayDat AS DATE);
GO

CREATE VIEW dbo.vw_SanPhamBanChay AS
SELECT sp.MaSanPham, sp.Ten, SUM(ct.SoLuong) AS SoLuongBan, SUM(ct.ThanhTien) AS DoanhThu
FROM dbo.ChiTietDonHang ct
JOIN dbo.DonHang dh ON dh.MaDonHang = ct.MaDonHang AND dh.TrangThai = 'HoanTat'
JOIN dbo.BienThe bt ON bt.MaBienThe = ct.MaBienThe
JOIN dbo.SanPham sp ON sp.MaSanPham = bt.MaSanPham
GROUP BY sp.MaSanPham, sp.Ten;
GO

-- Biến thể sắp hết hàng (≤ 10) và lô còn hàng sắp hết hạn (≤ 90 ngày)
CREATE VIEW dbo.vw_TonKhoThap AS
SELECT bt.MaBienThe, bt.MaSKU, sp.Ten AS TenSanPham, bt.SoLuongTon,
       (SELECT MIN(lo.HanSuDung) FROM dbo.LoHang lo
         WHERE lo.MaBienThe = bt.MaBienThe AND lo.SoLuongConLai > 0) AS HanSuDungGanNhat
FROM dbo.BienThe bt
JOIN dbo.SanPham sp ON sp.MaSanPham = bt.MaSanPham
WHERE bt.DangHoatDong = 1
  AND ( bt.SoLuongTon <= 10
     OR EXISTS (SELECT 1 FROM dbo.LoHang lo
                WHERE lo.MaBienThe = bt.MaBienThe AND lo.SoLuongConLai > 0
                  AND lo.HanSuDung <= DATEADD(DAY, 90, CAST(SYSUTCDATETIME() AS DATE))) );
GO

-- Số ý kiến tích cực / trung tính / tiêu cực theo sản phẩm × khía cạnh (công cụ của tác tử)
CREATE VIEW dbo.vw_ThongKeKhiaCanh AS
SELECT dg.MaSanPham, kc.MaKhiaCanh, kc.Ten AS TenKhiaCanh,
       SUM(CASE WHEN x.CamXuc = 'TichCuc'   THEN 1 ELSE 0 END) AS SoTichCuc,
       SUM(CASE WHEN x.CamXuc = 'TrungTinh' THEN 1 ELSE 0 END) AS SoTrungTinh,
       SUM(CASE WHEN x.CamXuc = 'TieuCuc'   THEN 1 ELSE 0 END) AS SoTieuCuc
FROM dbo.KetQuaKhiaCanh x
JOIN dbo.DanhGia dg  ON dg.MaDanhGia = x.MaDanhGia AND dg.TrangThai = 'DaDuyet'
JOIN dbo.KhiaCanh kc ON kc.MaKhiaCanh = x.MaKhiaCanh
GROUP BY dg.MaSanPham, kc.MaKhiaCanh, kc.Ten;
GO

-- Xu hướng cảm xúc theo sản phẩm theo ngày
CREATE VIEW dbo.vw_XuHuongCamXuc AS
SELECT MaSanPham, CAST(NgayTao AS DATE) AS Ngay,
       SUM(CASE WHEN CamXuc = 'TichCuc'   THEN 1 ELSE 0 END) AS SoTichCuc,
       SUM(CASE WHEN CamXuc = 'TrungTinh' THEN 1 ELSE 0 END) AS SoTrungTinh,
       SUM(CASE WHEN CamXuc = 'TieuCuc'   THEN 1 ELSE 0 END) AS SoTieuCuc
FROM dbo.DanhGia
WHERE TrangThai = 'DaDuyet' AND CamXuc IS NOT NULL
GROUP BY MaSanPham, CAST(NgayTao AS DATE);
GO

-- Chỉ số thực nghiệm theo mô hình: độ chính xác, precision / recall / F1 (macro), độ trễ
CREATE VIEW dbo.vw_ChiSoThucNghiem AS
WITH Lop AS (
    SELECT 'TichCuc' AS Nhan UNION ALL SELECT 'TrungTinh' UNION ALL SELECT 'TieuCuc'
), Dem AS (
    SELECT kq.MoHinh, l.Nhan,
           SUM(CASE WHEN kq.CamXucDuDoan = l.Nhan AND m.CamXucChuan =  l.Nhan THEN 1 ELSE 0 END) AS TP,
           SUM(CASE WHEN kq.CamXucDuDoan = l.Nhan AND m.CamXucChuan <> l.Nhan THEN 1 ELSE 0 END) AS FP,
           SUM(CASE WHEN kq.CamXucDuDoan <> l.Nhan AND m.CamXucChuan = l.Nhan THEN 1 ELSE 0 END) AS FN
    FROM dbo.KetQuaThucNghiem kq
    JOIN dbo.MauKiemThu m ON m.MaMau = kq.MaMau AND m.TapDuLieu = 'KiemThu'
    CROSS JOIN Lop l
    GROUP BY kq.MoHinh, l.Nhan
), TheoLop AS (
    SELECT MoHinh, Nhan,
           CASE WHEN TP + FP = 0 THEN 0.0 ELSE CAST(TP AS FLOAT) / (TP + FP) END AS P,
           CASE WHEN TP + FN = 0 THEN 0.0 ELSE CAST(TP AS FLOAT) / (TP + FN) END AS R
    FROM Dem
), Macro AS (
    SELECT MoHinh, AVG(P) AS DoChuanXacMacro, AVG(R) AS DoPhuMacro,
           AVG(CASE WHEN P + R = 0 THEN 0.0 ELSE 2 * P * R / (P + R) END) AS DiemF1Macro
    FROM TheoLop GROUP BY MoHinh
), Chung AS (
    SELECT kq.MoHinh, COUNT(*) AS SoMau,
           AVG(CAST(kq.LaDung AS FLOAT)) AS DoChinhXac,
           AVG(CAST(kq.DoTreMs AS FLOAT)) AS DoTreTrungBinhMs
    FROM dbo.KetQuaThucNghiem kq
    JOIN dbo.MauKiemThu m ON m.MaMau = kq.MaMau AND m.TapDuLieu = 'KiemThu'
    GROUP BY kq.MoHinh
)
SELECT c.MoHinh, c.SoMau, c.DoChinhXac, m.DoChuanXacMacro, m.DoPhuMacro, m.DiemF1Macro, c.DoTreTrungBinhMs
FROM Chung c JOIN Macro m ON m.MoHinh = c.MoHinh;
GO

/* ---------- Trigger: cập nhật điểm sao trung bình của sản phẩm ---------- */
CREATE TRIGGER dbo.trg_DanhGia_CapNhatDiem
ON dbo.DanhGia
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH ThayDoi AS (
        SELECT MaSanPham FROM inserted
        UNION
        SELECT MaSanPham FROM deleted
    )
    UPDATE sp
    SET sp.DiemTrungBinh = ISNULL(tk.DiemTB, 0),
        sp.SoDanhGia     = ISNULL(tk.SoLuong, 0)
    FROM dbo.SanPham sp
    JOIN ThayDoi td ON td.MaSanPham = sp.MaSanPham
    OUTER APPLY (
        SELECT CAST(AVG(CAST(dg.SoSao AS DECIMAL(4,2))) AS DECIMAL(3,2)) AS DiemTB, COUNT(*) AS SoLuong
        FROM dbo.DanhGia dg
        WHERE dg.MaSanPham = sp.MaSanPham AND dg.TrangThai = 'DaDuyet'
    ) tk;
END;
GO

/* ---------- Thủ tục: đổi trạng thái đơn hàng theo đúng luồng ----------
   ChoXacNhan -> DaXacNhan -> DangDongGoi -> DangGiao -> HoanTat
   ChoXacNhan / DaXacNhan / DangDongGoi -> DaHuy (hoàn lại kho)          */
CREATE PROCEDURE dbo.sp_DoiTrangThaiDonHang
    @MaDonHang    INT,
    @TrangThaiMoi VARCHAR(20),
    @MaNguoiDoi   INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @TrangThaiCu VARCHAR(20), @MaKhach INT;

    BEGIN TRAN;
    SELECT @TrangThaiCu = TrangThai, @MaKhach = MaNguoiDung
    FROM dbo.DonHang WITH (UPDLOCK, ROWLOCK)
    WHERE MaDonHang = @MaDonHang;

    IF @TrangThaiCu IS NULL
        THROW 50001, N'Không tìm thấy đơn hàng.', 1;

    IF NOT ( (@TrangThaiCu = 'ChoXacNhan'  AND @TrangThaiMoi IN ('DaXacNhan','DaHuy'))
          OR (@TrangThaiCu = 'DaXacNhan'   AND @TrangThaiMoi IN ('DangDongGoi','DaHuy'))
          OR (@TrangThaiCu = 'DangDongGoi' AND @TrangThaiMoi IN ('DangGiao','DaHuy'))
          OR (@TrangThaiCu = 'DangGiao'    AND @TrangThaiMoi = 'HoanTat') )
        THROW 50002, N'Chuyển trạng thái đơn hàng không hợp lệ.', 1;

    UPDATE dbo.DonHang
    SET TrangThai   = @TrangThaiMoi,
        DaThanhToan = CASE WHEN @TrangThaiMoi = 'HoanTat' THEN 1 ELSE DaThanhToan END,
        NgayCapNhat = SYSUTCDATETIME()
    WHERE MaDonHang = @MaDonHang;

    IF @TrangThaiMoi = 'DaHuy'
    BEGIN
        UPDATE lo SET lo.SoLuongConLai = lo.SoLuongConLai + ct.SoLuong
        FROM dbo.LoHang lo
        JOIN dbo.ChiTietDonHang ct ON ct.MaLo = lo.MaLo
        WHERE ct.MaDonHang = @MaDonHang;

        UPDATE bt SET bt.SoLuongTon = bt.SoLuongTon + q.SoLuong
        FROM dbo.BienThe bt
        JOIN (SELECT MaBienThe, SUM(SoLuong) AS SoLuong FROM dbo.ChiTietDonHang
              WHERE MaDonHang = @MaDonHang GROUP BY MaBienThe) q ON q.MaBienThe = bt.MaBienThe;
    END;

    INSERT dbo.ThongBao(MaNguoiDung, NoiDung, LienKet)
    VALUES (@MaKhach,
            N'Đơn hàng #' + CAST(@MaDonHang AS NVARCHAR(20)) + N' chuyển sang trạng thái ' + @TrangThaiMoi + N'.',
            N'/don-hang/' + CAST(@MaDonHang AS NVARCHAR(20)));

    INSERT dbo.NhatKyHeThong(MaNguoiDung, HanhDong, DoiTuong, ChiTiet)
    VALUES (@MaNguoiDoi, 'DOI_TRANG_THAI_DON', 'DonHang#' + CAST(@MaDonHang AS VARCHAR(20)),
            N'{"TrangThaiCu":"' + @TrangThaiCu + N'","TrangThaiMoi":"' + @TrangThaiMoi + N'"}');
    COMMIT;
END;
GO

/* ---------- Mô tả bảng/cột (MS_Description) ---------- */
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Loại da', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoaiDa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoaiDa', @level2type=N'COLUMN', @level2name=N'MaLoaiDa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Da dầu, da khô, da hỗn hợp, da nhạy cảm, da thường', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoaiDa', @level2type=N'COLUMN', @level2name=N'Ten';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Người dùng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'MaNguoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Email đăng nhập', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'Email';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mật khẩu đã băm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'MatKhauBam';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Họ tên', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'HoTen';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số điện thoại', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'SoDienThoai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Địa chỉ nhận hàng mặc định', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'DiaChi';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Loại da khai báo trong hồ sơ', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'MaLoaiDa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Phân quyền: khách hàng / quản trị viên', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'VaiTro';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Chuỗi băm của mã quên mật khẩu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'MaDatLaiMatKhau';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Hạn dùng của mã quên mật khẩu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'HanMaDatLai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tài khoản còn hoạt động', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'DangHoatDong';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm tạo (UTC)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NguoiDung', @level2type=N'COLUMN', @level2name=N'NgayTao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Danh mục', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhMuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhMuc', @level2type=N'COLUMN', @level2name=N'MaDanhMuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Danh mục cha (NULL = gốc)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhMuc', @level2type=N'COLUMN', @level2name=N'MaDanhMucCha';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tên danh mục', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhMuc', @level2type=N'COLUMN', @level2name=N'Ten';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đường dẫn thân thiện', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhMuc', @level2type=N'COLUMN', @level2name=N'DuongDan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thương hiệu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThuongHieu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThuongHieu', @level2type=N'COLUMN', @level2name=N'MaThuongHieu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tên thương hiệu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThuongHieu', @level2type=N'COLUMN', @level2name=N'Ten';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Xuất xứ thương hiệu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThuongHieu', @level2type=N'COLUMN', @level2name=N'QuocGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sản phẩm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'MaSanPham';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Danh mục', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'MaDanhMuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thương hiệu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'MaThuongHieu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tên sản phẩm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'Ten';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đường dẫn thân thiện', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'DuongDan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mô tả chi tiết', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'MoTa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Công dụng, cách nhau bằng dấu phẩy (dùng để lọc)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'CongDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thành phần dạng mô tả', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'ThanhPhan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Hướng dẫn sử dụng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'HuongDanSuDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thư viện ảnh: mảng JSON đường dẫn ảnh', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'DanhSachAnh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Trạng thái hiển thị', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'TrangThai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Điểm sao trung bình (trigger tự cập nhật)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'DiemTrungBinh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số đánh giá đã duyệt', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'SoDanhGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm tạo (UTC)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham', @level2type=N'COLUMN', @level2name=N'NgayTao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sản phẩm – loại da phù hợp', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham_LoaiDa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sản phẩm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham_LoaiDa', @level2type=N'COLUMN', @level2name=N'MaSanPham';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Loại da phù hợp', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'SanPham_LoaiDa', @level2type=N'COLUMN', @level2name=N'MaLoaiDa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Biến thể sản phẩm (SKU)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'MaBienThe';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sản phẩm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'MaSanPham';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã SKU', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'MaSKU';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Dung tích / quy cách (30ml, 50ml...)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'DungTich';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giá niêm yết (VNĐ)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'GiaBan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giá sale (nếu có)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'GiaKhuyenMai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giá sale áp dụng từ', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'KhuyenMaiTu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giá sale áp dụng đến', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'KhuyenMaiDen';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tồn kho khả dụng (tổng các lô)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'SoLuongTon';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Còn bán', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BienThe', @level2type=N'COLUMN', @level2name=N'DangHoatDong';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Lô hàng & hạn sử dụng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoHang', @level2type=N'COLUMN', @level2name=N'MaLo';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Biến thể', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoHang', @level2type=N'COLUMN', @level2name=N'MaBienThe';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số lô', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoHang', @level2type=N'COLUMN', @level2name=N'SoLo';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Hạn sử dụng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoHang', @level2type=N'COLUMN', @level2name=N'HanSuDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số lượng nhập', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoHang', @level2type=N'COLUMN', @level2name=N'SoLuongNhap';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số lượng còn lại', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoHang', @level2type=N'COLUMN', @level2name=N'SoLuongConLai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ngày nhập kho', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'LoHang', @level2type=N'COLUMN', @level2name=N'NgayNhap';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Bộ chăm sóc da (routine bundle)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BoChamSoc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BoChamSoc', @level2type=N'COLUMN', @level2name=N'MaBo';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tên bộ, ví dụ: Quy trình cho da dầu mụn', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BoChamSoc', @level2type=N'COLUMN', @level2name=N'Ten';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mô tả quy trình', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BoChamSoc', @level2type=N'COLUMN', @level2name=N'MoTa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Loại da mục tiêu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BoChamSoc', @level2type=N'COLUMN', @level2name=N'MaLoaiDa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giảm giá khi mua cả bộ (%)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BoChamSoc', @level2type=N'COLUMN', @level2name=N'PhanTramGiam';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Các bước trong bộ chăm sóc', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietBoChamSoc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Bộ chăm sóc', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietBoChamSoc', @level2type=N'COLUMN', @level2name=N'MaBo';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sản phẩm dùng ở bước này', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietBoChamSoc', @level2type=N'COLUMN', @level2name=N'MaBienThe';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thứ tự bước', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietBoChamSoc', @level2type=N'COLUMN', @level2name=N'ThuTuBuoc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sữa rửa mặt, tinh chất, chống nắng...', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietBoChamSoc', @level2type=N'COLUMN', @level2name=N'TenBuoc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Danh sách yêu thích', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'YeuThich';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khách hàng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'YeuThich', @level2type=N'COLUMN', @level2name=N'MaNguoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sản phẩm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'YeuThich', @level2type=N'COLUMN', @level2name=N'MaSanPham';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm tạo (UTC)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'YeuThich', @level2type=N'COLUMN', @level2name=N'NgayTao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giỏ hàng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'GioHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khách hàng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'GioHang', @level2type=N'COLUMN', @level2name=N'MaNguoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Biến thể', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'GioHang', @level2type=N'COLUMN', @level2name=N'MaBienThe';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số lượng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'GioHang', @level2type=N'COLUMN', @level2name=N'SoLuong';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm thêm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'GioHang', @level2type=N'COLUMN', @level2name=N'NgayThem';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Phiếu giảm giá (voucher)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia', @level2type=N'COLUMN', @level2name=N'MaPhieu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã khách nhập khi thanh toán', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia', @level2type=N'COLUMN', @level2name=N'MaCode';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giảm theo % hoặc số tiền', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia', @level2type=N'COLUMN', @level2name=N'KieuGiam';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'% hoặc số tiền giảm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia', @level2type=N'COLUMN', @level2name=N'GiaTriGiam';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số tiền giảm tối đa (với %)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia', @level2type=N'COLUMN', @level2name=N'GiamToiDa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giá trị đơn tối thiểu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia', @level2type=N'COLUMN', @level2name=N'DonToiThieu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số lượt còn dùng được (NULL = không giới hạn)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia', @level2type=N'COLUMN', @level2name=N'SoLuotConLai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Bắt đầu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia', @level2type=N'COLUMN', @level2name=N'BatDau';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Kết thúc', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PhieuGiamGia', @level2type=N'COLUMN', @level2name=N'KetThuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khuyến mãi theo sản phẩm / danh mục', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhuyenMai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhuyenMai', @level2type=N'COLUMN', @level2name=N'MaKhuyenMai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tên chương trình', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhuyenMai', @level2type=N'COLUMN', @level2name=N'Ten';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Phần trăm giảm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhuyenMai', @level2type=N'COLUMN', @level2name=N'PhanTramGiam';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Áp cho sản phẩm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhuyenMai', @level2type=N'COLUMN', @level2name=N'MaSanPham';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Áp cho danh mục', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhuyenMai', @level2type=N'COLUMN', @level2name=N'MaDanhMuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Bắt đầu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhuyenMai', @level2type=N'COLUMN', @level2name=N'BatDau';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Kết thúc', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhuyenMai', @level2type=N'COLUMN', @level2name=N'KetThuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đơn hàng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'MaDonHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khách đặt', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'MaNguoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Phiếu giảm giá đã áp', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'MaPhieu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Người nhận', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'TenNguoiNhan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SĐT người nhận', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'SoDienThoai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Địa chỉ giao hàng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'DiaChiGiaoHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giao tiêu chuẩn / giao nhanh', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'PhuongThucGiaoHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thanh toán khi nhận / chuyển khoản giả lập', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'PhuongThucThanhToan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tổng tiền hàng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'TienHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tiền giảm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'TienGiam';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Phí vận chuyển (mô phỏng)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'PhiVanChuyen';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khách phải trả', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'TongTien';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Trạng thái đơn', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'TrangThai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đã thanh toán', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'DaThanhToan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm đặt', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'NgayDat';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Lần đổi trạng thái gần nhất', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DonHang', @level2type=N'COLUMN', @level2name=N'NgayCapNhat';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Chi tiết đơn hàng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietDonHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietDonHang', @level2type=N'COLUMN', @level2name=N'MaChiTietDH';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đơn hàng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietDonHang', @level2type=N'COLUMN', @level2name=N'MaDonHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Biến thể', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietDonHang', @level2type=N'COLUMN', @level2name=N'MaBienThe';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Lô xuất kho (hết hạn trước xuất trước)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietDonHang', @level2type=N'COLUMN', @level2name=N'MaLo';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đơn giá lúc mua', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietDonHang', @level2type=N'COLUMN', @level2name=N'DonGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số lượng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietDonHang', @level2type=N'COLUMN', @level2name=N'SoLuong';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thành tiền', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ChiTietDonHang', @level2type=N'COLUMN', @level2name=N'ThanhTien';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khía cạnh / tiêu chí đánh giá', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhiaCanh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhiaCanh', @level2type=N'COLUMN', @level2name=N'MaKhiaCanh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mùi hương, độ thấm, bao bì, giá, kích ứng, hiệu quả', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhiaCanh', @level2type=N'COLUMN', @level2name=N'Ten';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Định nghĩa dùng khi gán nhãn và viết lời nhắc cho LLM', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KhiaCanh', @level2type=N'COLUMN', @level2name=N'MoTa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đánh giá sản phẩm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'MaDanhGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sản phẩm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'MaSanPham';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Người viết', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'MaNguoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Dòng đơn đã mua (xác thực đã mua hàng)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'MaChiTietDH';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số sao', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'SoSao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nội dung đánh giá', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'NoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đã mua hàng thật (verified purchase)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'DaMuaHang';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Kiểm duyệt', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'TrangThai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Cảm xúc tổng do AI phân loại', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'CamXuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Độ tin cậy của nhãn cảm xúc', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'DoTinCay';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã điểm vector của đánh giá trong Qdrant', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'MaVectorQdrant';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm tạo (UTC)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'NgayTao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Lần sửa gần nhất', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DanhGia', @level2type=N'COLUMN', @level2name=N'NgayCapNhat';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Điểm theo tiêu chí', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DiemTieuChi';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đánh giá', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DiemTieuChi', @level2type=N'COLUMN', @level2name=N'MaDanhGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tiêu chí', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DiemTieuChi', @level2type=N'COLUMN', @level2name=N'MaKhiaCanh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Điểm 1–5', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'DiemTieuChi', @level2type=N'COLUMN', @level2name=N'Diem';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Báo cáo đánh giá không phù hợp', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BaoCaoDanhGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BaoCaoDanhGia', @level2type=N'COLUMN', @level2name=N'MaBaoCao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đánh giá bị báo cáo', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BaoCaoDanhGia', @level2type=N'COLUMN', @level2name=N'MaDanhGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Người báo cáo', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BaoCaoDanhGia', @level2type=N'COLUMN', @level2name=N'MaNguoiBaoCao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Lý do', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BaoCaoDanhGia', @level2type=N'COLUMN', @level2name=N'LyDo';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Kết quả xử lý', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BaoCaoDanhGia', @level2type=N'COLUMN', @level2name=N'TrangThai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm tạo (UTC)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'BaoCaoDanhGia', @level2type=N'COLUMN', @level2name=N'NgayTao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thông báo', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThongBao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThongBao', @level2type=N'COLUMN', @level2name=N'MaThongBao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Người nhận', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThongBao', @level2type=N'COLUMN', @level2name=N'MaNguoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nội dung (đổi trạng thái đơn, khuyến mãi...)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThongBao', @level2type=N'COLUMN', @level2name=N'NoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đường dẫn liên quan', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThongBao', @level2type=N'COLUMN', @level2name=N'LienKet';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đã đọc', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThongBao', @level2type=N'COLUMN', @level2name=N'DaDoc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm tạo (UTC)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ThongBao', @level2type=N'COLUMN', @level2name=N'NgayTao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nhật ký thao tác (audit log)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyHeThong';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyHeThong', @level2type=N'COLUMN', @level2name=N'MaNhatKy';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Người thực hiện', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyHeThong', @level2type=N'COLUMN', @level2name=N'MaNguoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'DANG_NHAP, SUA_SAN_PHAM, DUYET_DANH_GIA, DOI_TRANG_THAI_DON...', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyHeThong', @level2type=N'COLUMN', @level2name=N'HanhDong';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Bảng và khóa của đối tượng, ví dụ DonHang#12', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyHeThong', @level2type=N'COLUMN', @level2name=N'DoiTuong';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Giá trị trước / sau (JSON)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyHeThong', @level2type=N'COLUMN', @level2name=N'ChiTiet';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm tạo (UTC)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyHeThong', @level2type=N'COLUMN', @level2name=N'NgayTao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Kết quả cảm xúc theo khía cạnh (ABSA)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaKhiaCanh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaKhiaCanh', @level2type=N'COLUMN', @level2name=N'MaKetQua';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đánh giá', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaKhiaCanh', @level2type=N'COLUMN', @level2name=N'MaDanhGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khía cạnh', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaKhiaCanh', @level2type=N'COLUMN', @level2name=N'MaKhiaCanh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Cảm xúc với khía cạnh', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaKhiaCanh', @level2type=N'COLUMN', @level2name=N'CamXuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Cụm từ trích từ đánh giá làm dẫn chứng', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaKhiaCanh', @level2type=N'COLUMN', @level2name=N'CumTuDanChung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mô hình đã dùng để phân tích', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaKhiaCanh', @level2type=N'COLUMN', @level2name=N'MoHinh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm tạo (UTC)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaKhiaCanh', @level2type=N'COLUMN', @level2name=N'NgayTao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tóm tắt ưu/nhược theo khía cạnh', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'MaTomTat';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sản phẩm', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'MaSanPham';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khía cạnh', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'MaKhiaCanh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số ý kiến tích cực', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'SoTichCuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Số ý kiến tiêu cực', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'SoTieuCuc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ưu điểm do LLM tóm tắt', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'UuDiem';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nhược điểm do LLM tóm tắt', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'NhuocDiem';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Vấn đề bị nhắc lại nhiều lần (nếu có)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'VanDeLapLai';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã các đánh giá làm dẫn chứng (JSON)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'DanChung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Lần tóm tắt gần nhất', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TomTatKhiaCanh', @level2type=N'COLUMN', @level2name=N'NgayCapNhat';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tin nhắn hỏi đáp với tác tử', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan', @level2type=N'COLUMN', @level2name=N'MaTinNhan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mã phiên trò chuyện', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan', @level2type=N'COLUMN', @level2name=N'MaPhien';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Người hỏi (NULL = khách vãng lai)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan', @level2type=N'COLUMN', @level2name=N'MaNguoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sản phẩm đang hỏi', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan', @level2type=N'COLUMN', @level2name=N'MaSanPham';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Người dùng hỏi / tác tử trả lời', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan', @level2type=N'COLUMN', @level2name=N'NguoiGui';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nội dung', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan', @level2type=N'COLUMN', @level2name=N'NoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đoạn trích và mã đánh giá làm dẫn chứng (JSON)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan', @level2type=N'COLUMN', @level2name=N'DanChung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời gian trả lời (ms)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan', @level2type=N'COLUMN', @level2name=N'DoTreMs';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm tạo (UTC)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'TinNhan', @level2type=N'COLUMN', @level2name=N'NgayTao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nhật ký các bước của tác tử (trace)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyTacTu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyTacTu', @level2type=N'COLUMN', @level2name=N'MaBuoc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Câu trả lời được ghi vết', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyTacTu', @level2type=N'COLUMN', @level2name=N'MaTinNhan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thứ tự bước', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyTacTu', @level2type=N'COLUMN', @level2name=N'BuocSo';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Loại bước', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyTacTu', @level2type=N'COLUMN', @level2name=N'LoaiBuoc';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'tim_danh_gia, thong_ke_khia_canh...', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyTacTu', @level2type=N'COLUMN', @level2name=N'TenCongCu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tham số đầu vào (JSON)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyTacTu', @level2type=N'COLUMN', @level2name=N'DauVao';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Kết quả trả về (JSON)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyTacTu', @level2type=N'COLUMN', @level2name=N'DauRa';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời gian bước (ms)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'NhatKyTacTu', @level2type=N'COLUMN', @level2name=N'DoTreMs';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mẫu trong bộ kiểm thử (test set)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'MauKiemThu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'MauKiemThu', @level2type=N'COLUMN', @level2name=N'MaMau';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Đánh giá nguồn (nếu lấy từ hệ thống)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'MauKiemThu', @level2type=N'COLUMN', @level2name=N'MaDanhGia';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Văn bản đánh giá', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'MauKiemThu', @level2type=N'COLUMN', @level2name=N'NoiDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nhãn cảm xúc đúng (gán tay)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'MauKiemThu', @level2type=N'COLUMN', @level2name=N'CamXucChuan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nhãn khía cạnh đúng (JSON)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'MauKiemThu', @level2type=N'COLUMN', @level2name=N'KhiaCanhChuan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mẫu kiểm thử / ví dụ mẫu cho LLM ít ví dụ', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'MauKiemThu', @level2type=N'COLUMN', @level2name=N'TapDuLieu';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Kết quả thực nghiệm từng mẫu', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khóa chính', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem', @level2type=N'COLUMN', @level2name=N'MaKetQua';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Mẫu kiểm thử', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem', @level2type=N'COLUMN', @level2name=N'MaMau';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PhoBERT / LLM không ví dụ / LLM ít ví dụ', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem', @level2type=N'COLUMN', @level2name=N'MoHinh';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nhãn dự đoán', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem', @level2type=N'COLUMN', @level2name=N'CamXucDuDoan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Khía cạnh dự đoán (JSON)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem', @level2type=N'COLUMN', @level2name=N'KhiaCanhDuDoan';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Dự đoán cảm xúc đúng hay sai', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem', @level2type=N'COLUMN', @level2name=N'LaDung';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nhóm lỗi khi sai: phủ định, mỉa mai, sai khía cạnh...', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem', @level2type=N'COLUMN', @level2name=N'NhomLoi';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời gian dự đoán (ms)', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem', @level2type=N'COLUMN', @level2name=N'DoTreMs';
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Thời điểm chạy', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'KetQuaThucNghiem', @level2type=N'COLUMN', @level2name=N'NgayChay';
GO

/* =====================================================================
   DỮ LIỆU MẪU – chạy trên CSDL vừa tạo, cột tự tăng bắt đầu từ 1.
   Mật khẩu mẫu là chuỗi giữ chỗ; kết quả thực nghiệm chỉ để minh họa
   cách tính chỉ số, không phải kết quả thật.
   ===================================================================== */
SET XACT_ABORT ON;
BEGIN TRAN;

INSERT dbo.LoaiDa(Ten) VALUES (N'Da dầu'), (N'Da khô'), (N'Da hỗn hợp'), (N'Da nhạy cảm'), (N'Da thường');

INSERT dbo.NguoiDung(Email, MatKhauBam, HoTen, SoDienThoai, DiaChi, MaLoaiDa, VaiTro) VALUES
 (N'quantri@ria.local',      '$2b$12$MAT_KHAU_MAU_QT', N'Quản trị viên',  '0900000000', NULL, NULL, 'QuanTri'),
 (N'lan.nguyen@example.com', '$2b$12$MAT_KHAU_MAU_1',  N'Nguyễn Thị Lan', '0912345678', N'12 Lê Lợi, Bến Nghé, TP. Hồ Chí Minh', 1, 'KhachHang'),
 (N'minh.tran@example.com',  '$2b$12$MAT_KHAU_MAU_2',  N'Trần Văn Minh',  '0987654321', N'45 Xuân Thủy, Dịch Vọng, Hà Nội', 4, 'KhachHang');

INSERT dbo.DanhMuc(MaDanhMucCha, Ten, DuongDan) VALUES (NULL, N'Chăm sóc da mặt', 'cham-soc-da-mat');
INSERT dbo.DanhMuc(MaDanhMucCha, Ten, DuongDan) VALUES
 (1, N'Sữa rửa mặt', 'sua-rua-mat'), (1, N'Tinh chất', 'tinh-chat'),
 (1, N'Kem dưỡng', 'kem-duong'), (1, N'Kem chống nắng', 'kem-chong-nang');

INSERT dbo.ThuongHieu(Ten, QuocGia) VALUES
 (N'Cocoon', N'Việt Nam'), (N'Some By Mi', N'Hàn Quốc'), (N'La Roche-Posay', N'Pháp'), (N'Innisfree', N'Hàn Quốc');

INSERT dbo.SanPham(MaDanhMuc, MaThuongHieu, Ten, DuongDan, CongDung, ThanhPhan, DanhSachAnh) VALUES
 (2, 1, N'Gel rửa mặt bí đao',             'gel-rua-mat-bi-dao',       N'Làm sạch, kiểm soát dầu',   N'Chiết xuất bí đao, rau má', N'["/anh/gel-bi-dao-1.jpg","/anh/gel-bi-dao-2.jpg"]'),
 (3, 2, N'Tinh chất Niacinamide 10%',      'tinh-chat-niacinamide-10', N'Mờ thâm, se khít lỗ chân lông', N'Niacinamide 10%, HA', N'["/anh/niacinamide-1.jpg"]'),
 (4, 3, N'Kem dưỡng phục hồi B5',          'kem-duong-phuc-hoi-b5',    N'Dưỡng ẩm, phục hồi da',     N'Panthenol 5%, Ceramide', N'["/anh/kem-b5-1.jpg"]'),
 (5, 4, N'Kem chống nắng kiềm dầu SPF50+', 'kem-chong-nang-kiem-dau',  N'Chống nắng, kiểm soát dầu', N'Kẽm oxit, chiết xuất trà xanh', N'["/anh/kcn-1.jpg"]');

INSERT dbo.SanPham_LoaiDa(MaSanPham, MaLoaiDa) VALUES (1,1),(1,3),(2,1),(2,3),(2,5),(3,2),(3,4),(4,1),(4,3);

INSERT dbo.BienThe(MaSanPham, MaSKU, DungTich, GiaBan, GiaKhuyenMai, KhuyenMaiTu, KhuyenMaiDen, SoLuongTon) VALUES
 (1, 'COC-GEL-140', N'140ml', 145000, NULL,   NULL, NULL, 120),
 (2, 'SBM-NIA-30',  N'30ml',  320000, 272000, '2026-11-27', '2026-11-30 23:59:59', 79),
 (3, 'LRP-B5-40',   N'40ml',  385000, NULL,   NULL, NULL, 48),
 (3, 'LRP-B5-100',  N'100ml', 750000, NULL,   NULL, NULL, 8),
 (4, 'INN-SUN-50',  N'50ml',  425000, NULL,   NULL, NULL, 39);

INSERT dbo.LoHang(MaBienThe, SoLo, HanSuDung, SoLuongNhap, SoLuongConLai) VALUES
 (1, 'L2601', '2028-01-10', 120, 120),
 (2, 'L2603', '2028-03-05',  80,  79),
 (3, 'L2512', '2027-02-15',  50,  48),
 (4, 'L2506', '2027-01-20',   8,   8),
 (5, 'L2604', '2029-04-20',  40,  39);

INSERT dbo.BoChamSoc(Ten, MoTa, MaLoaiDa, PhanTramGiam) VALUES
 (N'Quy trình cơ bản cho da dầu mụn', N'Làm sạch – đặc trị – chống nắng', 1, 10);
INSERT dbo.ChiTietBoChamSoc(MaBo, MaBienThe, ThuTuBuoc, TenBuoc) VALUES
 (1, 1, 1, N'Sữa rửa mặt'), (1, 2, 2, N'Tinh chất đặc trị'), (1, 5, 3, N'Chống nắng');

INSERT dbo.YeuThich(MaNguoiDung, MaSanPham) VALUES (2, 3), (3, 2);
INSERT dbo.GioHang(MaNguoiDung, MaBienThe, SoLuong) VALUES (2, 1, 1);

INSERT dbo.PhieuGiamGia(MaCode, KieuGiam, GiaTriGiam, GiamToiDa, DonToiThieu, SoLuotConLai, BatDau, KetThuc) VALUES
 ('CHAOBAN10', 'PhanTram', 10, 50000, 300000, 999, '2026-01-01', '2026-12-31 23:59:59');
INSERT dbo.KhuyenMai(Ten, PhanTramGiam, MaSanPham, MaDanhMuc, BatDau, KetThuc) VALUES
 (N'Black Friday – tinh chất giảm 15%', 15, NULL, 3, '2026-11-27', '2026-11-30 23:59:59');

-- Đơn 1 hoàn tất: 745.000 – 50.000 + 30.000 = 725.000
INSERT dbo.DonHang(MaNguoiDung, MaPhieu, TenNguoiNhan, SoDienThoai, DiaChiGiaoHang, PhuongThucGiaoHang, PhuongThucThanhToan,
                   TienHang, TienGiam, PhiVanChuyen, TongTien, TrangThai, DaThanhToan, NgayDat, NgayCapNhat)
VALUES (2, 1, N'Nguyễn Thị Lan', '0912345678', N'12 Lê Lợi, Bến Nghé, TP. Hồ Chí Minh', 'TieuChuan', 'TienMat',
        745000, 50000, 30000, 725000, 'HoanTat', 1, '2026-11-02 09:15:00', '2026-11-06 16:40:00');
-- Đơn 2 chờ xác nhận: miễn phí vận chuyển vì ≥ 500.000
INSERT dbo.DonHang(MaNguoiDung, TenNguoiNhan, SoDienThoai, DiaChiGiaoHang, PhuongThucGiaoHang, PhuongThucThanhToan,
                   TienHang, TienGiam, PhiVanChuyen, TongTien, NgayDat)
VALUES (3, N'Trần Văn Minh', '0987654321', N'45 Xuân Thủy, Dịch Vọng, Hà Nội', 'TieuChuan', 'ChuyenKhoanGiaLap',
        770000, 0, 0, 770000, '2026-11-21 20:05:00');

INSERT dbo.ChiTietDonHang(MaDonHang, MaBienThe, MaLo, DonGia, SoLuong, ThanhTien) VALUES
 (1, 2, 2, 320000, 1, 320000),
 (1, 5, 5, 425000, 1, 425000),
 (2, 3, 3, 385000, 2, 770000);

INSERT dbo.KhiaCanh(Ten, MoTa) VALUES
 (N'Mùi hương', N'Mùi thơm, mùi hắc, không mùi'),
 (N'Độ thấm',   N'Thấm nhanh/chậm, bết dính, nhờn'),
 (N'Bao bì',    N'Chai, vòi, nắp, độ chắc chắn, thẩm mỹ'),
 (N'Giá',       N'Giá so với dung tích và chất lượng'),
 (N'Kích ứng',  N'Châm chích, đỏ, mẩn, nổi mụn'),
 (N'Hiệu quả',  N'Hiệu quả cảm nhận: mờ thâm, giảm mụn, kiềm dầu...');

INSERT dbo.DanhGia(MaSanPham, MaNguoiDung, MaChiTietDH, SoSao, NoiDung, DaMuaHang, TrangThai, CamXuc, DoTinCay, MaVectorQdrant, NgayTao) VALUES
 (2, 2, 1, 4, N'Tinh chất thấm nhanh, không bết dính. Sau 3 tuần vết thâm mờ rõ. Mùi hơi hắc nhưng chấp nhận được.',
  1, 'DaDuyet', 'TichCuc', 0.9100, '0b6f2a52-6c1e-4f43-9a51-1f0d7e3a0001', '2026-11-20 21:00:00'),
 (4, 2, 2, 2, N'Kiềm dầu tốt nhưng dùng 2 ngày thì da bị châm chích, nổi mẩn đỏ. Giá hơi cao so với dung tích.',
  1, 'DaDuyet', 'TieuCuc', 0.8800, '0b6f2a52-6c1e-4f43-9a51-1f0d7e3a0002', '2026-11-21 08:30:00'),
 (3, 3, NULL, 5, N'Kem dịu da, mùi nhẹ, bao bì dạng tuýp tiện mang đi.',
  0, 'ChoDuyet', NULL, NULL, NULL, '2026-11-22 10:00:00');

INSERT dbo.DiemTieuChi(MaDanhGia, MaKhiaCanh, Diem) VALUES (1,2,5),(1,1,3),(1,6,4),(2,5,1),(2,4,2),(2,6,4);
INSERT dbo.BaoCaoDanhGia(MaDanhGia, MaNguoiBaoCao, LyDo) VALUES (3, 2, N'Chưa mua hàng nhưng đánh giá 5 sao');

INSERT dbo.ThongBao(MaNguoiDung, NoiDung, LienKet) VALUES
 (2, N'Đơn hàng #1 đã giao thành công. Hãy đánh giá sản phẩm nhé!', N'/don-hang/1'),
 (2, N'Black Friday: tinh chất giảm 15% từ 27/11 đến 30/11.', N'/khuyen-mai');
INSERT dbo.NhatKyHeThong(MaNguoiDung, HanhDong, DoiTuong, ChiTiet) VALUES
 (1, 'DUYET_DANH_GIA', 'DanhGia#1', N'{"TrangThai":"DaDuyet"}'),
 (1, 'DUYET_DANH_GIA', 'DanhGia#2', N'{"TrangThai":"DaDuyet"}');

/* ---------- Phân tích AI, tác tử, thực nghiệm ---------- */
INSERT dbo.KetQuaKhiaCanh(MaDanhGia, MaKhiaCanh, CamXuc, CumTuDanChung, MoHinh) VALUES
 (1, 2, 'TichCuc',   N'thấm nhanh, không bết dính', 'LLM_ItVD'),
 (1, 6, 'TichCuc',   N'vết thâm mờ rõ', 'LLM_ItVD'),
 (1, 1, 'TrungTinh', N'Mùi hơi hắc nhưng chấp nhận được', 'LLM_ItVD'),
 (2, 6, 'TichCuc',   N'Kiềm dầu tốt', 'LLM_ItVD'),
 (2, 5, 'TieuCuc',   N'da bị châm chích, nổi mẩn đỏ', 'LLM_ItVD'),
 (2, 4, 'TieuCuc',   N'Giá hơi cao so với dung tích', 'LLM_ItVD');

INSERT dbo.TomTatKhiaCanh(MaSanPham, MaKhiaCanh, SoTichCuc, SoTieuCuc, UuDiem, NhuocDiem, VanDeLapLai, DanChung) VALUES
 (4, 5, 0, 1, NULL, N'Có phản ánh châm chích, nổi mẩn đỏ sau 2 ngày dùng.', N'Kích ứng với da dầu', N'[2]');

INSERT dbo.TinNhan(MaPhien, MaNguoiDung, MaSanPham, NguoiGui, NoiDung, DanChung, DoTreMs, NgayTao) VALUES
 ('7d1c0b3e-0000-4000-8000-000000000001', 2, 4, 'NguoiDung',
  N'Kem chống nắng này có gây kích ứng cho da nhạy cảm không?', NULL, NULL, '2026-11-22 20:00:00'),
 ('7d1c0b3e-0000-4000-8000-000000000001', 2, 4, 'TroLy',
  N'Trong các đánh giá đã duyệt, có 1 phản ánh da bị châm chích, nổi mẩn đỏ sau 2 ngày dùng [1]. Số đánh giá còn ít nên chưa đủ để kết luận.',
  N'[{"maDanhGia":2,"doanTrich":"dùng 2 ngày thì da bị châm chích, nổi mẩn đỏ","diem":0.81}]', 4800, '2026-11-22 20:00:05');
INSERT dbo.NhatKyTacTu(MaTinNhan, BuocSo, LoaiBuoc, TenCongCu, DauVao, DauRa, DoTreMs) VALUES
 (2, 1, 'LapKeHoach', NULL,                 N'{"khia_canh":"Kích ứng"}', NULL, 600),
 (2, 2, 'GoiCongCu',  'tim_danh_gia',       N'{"ma_san_pham":4,"truy_van":"kích ứng da nhạy cảm"}', N'{"so_ket_qua":1}', 350),
 (2, 3, 'GoiCongCu',  'thong_ke_khia_canh', N'{"ma_san_pham":4,"khia_canh":"Kích ứng"}', N'{"tich_cuc":0,"tieu_cuc":1}', 40),
 (2, 4, 'KiemSoat',   NULL,                 N'{"kiem_tra":"bat_buoc_trich_dan"}', N'{"dat":true}', 10),
 (2, 5, 'TraLoi',     NULL,                 NULL, N'{"so_trich_dan":1}', 3800);

INSERT dbo.MauKiemThu(MaDanhGia, NoiDung, CamXucChuan, KhiaCanhChuan, TapDuLieu) VALUES
 (1, N'Tinh chất thấm nhanh, không bết dính. Sau 3 tuần vết thâm mờ rõ. Mùi hơi hắc nhưng chấp nhận được.', 'TichCuc',
  N'{"Độ thấm":"TichCuc","Hiệu quả":"TichCuc","Mùi hương":"TrungTinh"}', 'KiemThu'),
 (2, N'Kiềm dầu tốt nhưng dùng 2 ngày thì da bị châm chích, nổi mẩn đỏ. Giá hơi cao so với dung tích.', 'TieuCuc',
  N'{"Hiệu quả":"TichCuc","Kích ứng":"TieuCuc","Giá":"TieuCuc"}', 'KiemThu'),
 (NULL, N'Chai bị rò ở vòi, dùng thì cũng bình thường.', 'TrungTinh', N'{"Bao bì":"TieuCuc","Hiệu quả":"TrungTinh"}', 'ViDuMau');
-- Minh họa: mỗi mô hình dự đoán 2 mẫu kiểm thử
INSERT dbo.KetQuaThucNghiem(MaMau, MoHinh, CamXucDuDoan, LaDung, NhomLoi, DoTreMs) VALUES
 (1, 'PhoBERT',     'TichCuc',   1, NULL, 40),
 (2, 'PhoBERT',     'TichCuc',   0, N'Câu có "nhưng" – bỏ sót vế tiêu cực', 38),
 (1, 'LLM_KhongVD', 'TichCuc',   1, NULL, 2100),
 (2, 'LLM_KhongVD', 'TrungTinh', 0, N'Cân bằng ý tích cực và tiêu cực', 2300),
 (1, 'LLM_ItVD',    'TichCuc',   1, NULL, 2600),
 (2, 'LLM_ItVD',    'TieuCuc',   1, NULL, 2700);

COMMIT;
GO

/* ---------- Kiểm tra nhanh ---------- */
SELECT * FROM dbo.vw_ThongKeKhiaCanh;
SELECT * FROM dbo.vw_ChiSoThucNghiem;
GO
