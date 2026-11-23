import type { KetQuaPhanTrang } from './common'
import type { DanhGia } from './danhGia'
import type { LoaiDa, ThuongHieu } from './traCuu'

/** Khớp SapXepSanPham ở backend. */
export type SapXepSanPham = 'moiNhat' | 'giaTang' | 'giaGiam' | 'danhGiaCao'

export interface TimKiemSanPhamParams {
  tuKhoa?: string
  maDanhMuc?: number
  maThuongHieu?: number
  maLoaiDa?: number
  giaTu?: number
  giaDen?: number
  sapXep?: SapXepSanPham
  trang?: number
  kichThuoc?: number
}

export interface SanPhamTomTat {
  maSanPham: number
  ten: string
  duongDan: string
  congDung: string | null
  anhDaiDien: string | null
  tenThuongHieu: string
  tenDanhMuc: string
  /** Giá thấp nhất đang áp dụng (đã tính khuyến mãi còn hiệu lực); null nếu chưa có biến thể. */
  giaThapNhat: number | null
  coKhuyenMai: boolean
  conHang: boolean
  diemTrungBinh: number
  soDanhGia: number
  /** Giá gốc (gạch ngang) khi biến thể rẻ nhất đang khuyến mãi. */
  giaGoc: number | null
  soBienThe: number
  /** Biến thể còn hàng rẻ nhất – dùng cho nút "Thêm vào giỏ" trên thẻ. */
  bienTheMacDinh: BienTheMacDinh | null
}

export interface BienTheMacDinh {
  maBienThe: number
  dungTich: string
  giaApDung: number
  giaGoc: number | null
  soLuongTon: number
}

/** dbo.BienThe – giaKhuyenMai chỉ khác null khi đang trong thời gian khuyến mãi. */
export interface BienThe {
  maBienThe: number
  maSKU: string
  dungTich: string
  giaBan: number
  giaKhuyenMai: number | null
  khuyenMaiDen: string | null
  giaApDung: number
  soLuongTon: number
  conHang: boolean
}

/** dbo.TomTatKhiaCanh – danChung là danh sách MaDanhGia. */
export interface TomTatKhiaCanh {
  maKhiaCanh: number
  tenKhiaCanh: string
  soTichCuc: number
  soTieuCuc: number
  uuDiem: string | null
  nhuocDiem: string | null
  vanDeLapLai: string | null
  danChung: number[]
  ngayCapNhat: string
}

export interface SanPhamChiTiet {
  maSanPham: number
  ten: string
  duongDan: string
  moTa: string | null
  congDung: string | null
  thanhPhan: string | null
  huongDanSuDung: string | null
  danhSachAnh: string[]
  trangThai: 'DangBan' | 'NgungBan'
  diemTrungBinh: number
  soDanhGia: number
  danhMuc: { maDanhMuc: number; ten: string; duongDan: string }
  thuongHieu: ThuongHieu
  loaiDaPhuHop: LoaiDa[]
  bienThe: BienThe[]
  tomTatKhiaCanh: TomTatKhiaCanh[]
  danhGia: KetQuaPhanTrang<DanhGia>
}

// ======================================================================= Quản trị sản phẩm

export type TrangThaiSanPham = 'DangBan' | 'NgungBan'

export interface SanPhamQuanTriParams {
  tuKhoa?: string
  trangThai?: TrangThaiSanPham
  maDanhMuc?: number
  maThuongHieu?: number
  trang?: number
  kichThuoc?: number
}

export interface SanPhamQuanTri {
  maSanPham: number
  ten: string
  duongDan: string
  anhDaiDien: string | null
  tenThuongHieu: string
  tenDanhMuc: string
  trangThai: TrangThaiSanPham
  soBienThe: number
  tongTon: number
  giaThapNhat: number | null
  diemTrungBinh: number
  soDanhGia: number
  ngayTao: string
}

export interface DanhSachSanPhamQuanTri {
  ketQua: KetQuaPhanTrang<SanPhamQuanTri>
  soLuongTheoTrangThai: Record<TrangThaiSanPham, number>
}

/** dbo.LoHang – hanSuDung dạng yyyy-MM-dd. */
export interface LoHang {
  maLo: number
  soLo: string
  hanSuDung: string
  soLuongNhap: number
  soLuongConLai: number
  ngayNhap: string
}

export interface BienTheQuanTri {
  maBienThe: number
  maSKU: string
  dungTich: string
  giaBan: number
  giaKhuyenMai: number | null
  khuyenMaiTu: string | null
  khuyenMaiDen: string | null
  soLuongTon: number
  dangHoatDong: boolean
  loHang: LoHang[]
}

export interface SanPhamQuanTriChiTiet {
  maSanPham: number
  maDanhMuc: number
  maThuongHieu: number
  ten: string
  duongDan: string
  moTa: string | null
  congDung: string | null
  thanhPhan: string | null
  huongDanSuDung: string | null
  danhSachAnh: string[]
  trangThai: TrangThaiSanPham
  diemTrungBinh: number
  soDanhGia: number
  ngayTao: string
  loaiDa: LoaiDa[]
  bienThe: BienTheQuanTri[]
}

export interface LuuSanPhamRequest {
  maDanhMuc: number
  maThuongHieu: number
  ten: string
  duongDan?: string
  moTa?: string
  congDung?: string
  thanhPhan?: string
  huongDanSuDung?: string
  danhSachAnh: string[]
  maLoaiDa: number[]
  trangThai: TrangThaiSanPham
}

export interface LuuBienTheRequest {
  maSKU: string
  dungTich: string
  giaBan: number
  giaKhuyenMai: number | null
  /** ISO UTC */
  khuyenMaiTu: string | null
  khuyenMaiDen: string | null
  dangHoatDong: boolean
}

export interface NhapLoHangRequest {
  soLo: string
  /** yyyy-MM-dd */
  hanSuDung: string
  soLuongNhap: number
}

export interface AnhDaTaiLen {
  duongDan: string
  tenTep: string
  kichThuoc: number
}
