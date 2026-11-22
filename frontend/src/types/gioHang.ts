import type { PhuongThucGiaoHang } from './donHang'

export interface DongGioHang {
  maBienThe: number
  maSanPham: number
  tenSanPham: string
  duongDan: string
  anhDaiDien: string | null
  dungTich: string
  maSKU: string
  giaBan: number
  /** Đơn giá hiện hành (đã tính khuyến mãi còn hiệu lực). */
  donGia: number
  dangKhuyenMai: boolean
  soLuong: number
  soLuongTon: number
  thanhTien: number
  /** false: ngừng kinh doanh / hết hàng. */
  coTheMua: boolean
  /** Số lượng trong giỏ lớn hơn tồn kho hiện tại. */
  vuotTonKho: boolean
}

export interface GioHang {
  danhSach: DongGioHang[]
  tongSoLuong: number
  tamTinh: number
  coTheDatHang: boolean
  phiVanChuyen: Record<PhuongThucGiaoHang, number>
}
