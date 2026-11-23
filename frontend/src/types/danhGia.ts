import type { KetQuaPhanTrang } from './common'

/** CK_DanhGia_CamXuc */
export type CamXuc = 'TichCuc' | 'TrungTinh' | 'TieuCuc'

/** CK_DanhGia_TrangThai */
export type TrangThaiDanhGia = 'ChoDuyet' | 'DaDuyet' | 'DaAn'

export interface DiemTieuChi {
  maKhiaCanh: number
  tenKhiaCanh: string
  diem: number
}

/** Đánh giá công khai (DaDuyet). */
export interface DanhGia {
  maDanhGia: number
  soSao: number
  noiDung: string
  daMuaHang: boolean
  camXuc: CamXuc | null
  ngayTao: string
  tenNguoiDanhGia: string
  dungTichDaMua: string | null
  diemTieuChi: DiemTieuChi[]
}

export interface TaoDanhGiaRequest {
  maSanPham: number
  soSao: number
  noiDung: string
  maChiTietDH?: number
  diemTieuChi: { maKhiaCanh: number; diem: number }[]
}

export interface DanhGiaDaTao {
  maDanhGia: number
  maSanPham: number
  trangThai: TrangThaiDanhGia
  daMuaHang: boolean
  ngayTao: string
}

export interface DongDonCoTheDanhGia {
  maChiTietDH: number
  maDonHang: number
  dungTich: string
  ngayDat: string
}

export interface DanhGiaQuanTri {
  maDanhGia: number
  maSanPham: number
  tenSanPham: string
  duongDanSanPham: string
  maNguoiDung: number
  tenNguoiDung: string
  email: string
  soSao: number
  noiDung: string
  daMuaHang: boolean
  trangThai: TrangThaiDanhGia
  camXuc: CamXuc | null
  ngayTao: string
  ngayCapNhat: string | null
  soBaoCaoChoXuLy: number
  diemTieuChi: DiemTieuChi[]
}

export interface DanhSachDanhGiaQuanTri {
  ketQua: KetQuaPhanTrang<DanhGiaQuanTri>
  soLuongTheoTrangThai: Record<TrangThaiDanhGia, number>
}

export interface DoiTrangThaiDanhGiaResponse {
  maDanhGia: number
  trangThaiCu: TrangThaiDanhGia
  trangThai: TrangThaiDanhGia
  sanPham: { maSanPham: number; diemTrungBinh: number; soDanhGia: number }
  daGuiPhanTichAi: boolean
  /** Đánh giá rời DaDuyet → đã gửi ai-service xóa vector Qdrant và tính lại tóm tắt. */
  daGuiGoBoVector: boolean
}

export interface BaoCaoDanhGia {
  maBaoCao: number
  maDanhGia: number
  lyDo: string
  trangThai: 'ChoXuLy' | 'DaXuLy' | 'BoQua'
  ngayTao: string
}
