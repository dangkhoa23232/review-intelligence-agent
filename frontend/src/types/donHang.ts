import type { KetQuaPhanTrang } from './common'

/** CK_DonHang_TrangThai */
export type TrangThaiDonHang = 'ChoXacNhan' | 'DaXacNhan' | 'DangDongGoi' | 'DangGiao' | 'HoanTat' | 'DaHuy'
/** CK_DonHang_PhuongThucGiaoHang */
export type PhuongThucGiaoHang = 'TieuChuan' | 'Nhanh'
/** CK_DonHang_PhuongThucThanhToan */
export type PhuongThucThanhToan = 'TienMat' | 'ChuyenKhoanGiaLap'

export interface KiemTraPhieuGiamGia {
  maCode: string
  kieuGiam: 'PhanTram' | 'SoTien'
  giaTriGiam: number
  giamToiDa: number | null
  donToiThieu: number
  ketThuc: string
  tienHang: number
  tienGiam: number
}

export interface DatHangRequest {
  tenNguoiNhan: string
  soDienThoai: string
  diaChiGiaoHang: string
  phuongThucGiaoHang: PhuongThucGiaoHang
  phuongThucThanhToan: PhuongThucThanhToan
  maCode?: string
}

export interface DongDonHang {
  maBienThe: number
  maSanPham: number
  tenSanPham: string
  duongDan: string
  anhDaiDien: string | null
  dungTich: string
  maSKU: string
  donGia: number
  soLuong: number
  thanhTien: number
  coTheDanhGia: boolean
}

export interface DonHangChiTiet {
  maDonHang: number
  ngayDat: string
  ngayCapNhat: string | null
  trangThai: TrangThaiDonHang
  daThanhToan: boolean
  tenNguoiNhan: string
  soDienThoai: string
  diaChiGiaoHang: string
  phuongThucGiaoHang: PhuongThucGiaoHang
  phuongThucThanhToan: PhuongThucThanhToan
  maCode: string | null
  tienHang: number
  tienGiam: number
  phiVanChuyen: number
  tongTien: number
  maNguoiDung: number
  tenKhachHang: string
  emailKhachHang: string
  sanPham: DongDonHang[]
  coTheHuy: boolean
  trangThaiTiepTheo: TrangThaiDonHang[]
}

export interface DonHangTomTat {
  maDonHang: number
  ngayDat: string
  trangThai: TrangThaiDonHang
  daThanhToan: boolean
  tongTien: number
  tongSoLuong: number
  soMatHang: number
  tenSanPhamDau: string | null
  anhDaiDien: string | null
}

export interface DanhSachDonHangQuanTri {
  ketQua: KetQuaPhanTrang<DonHangChiTiet>
  soLuongTheoTrangThai: Record<TrangThaiDonHang, number>
}
