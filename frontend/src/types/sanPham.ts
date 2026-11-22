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
