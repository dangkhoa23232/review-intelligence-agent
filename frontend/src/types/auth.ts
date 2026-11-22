/** Khớp bảng dbo.NguoiDung (JSON camelCase từ backend). */
export type VaiTro = 'KhachHang' | 'QuanTri'

export interface NguoiDung {
  maNguoiDung: number
  email: string
  hoTen: string
  vaiTro: VaiTro
  maLoaiDa: number | null
}

/** GET /api/auth/toi – hồ sơ đầy đủ để tự điền form thanh toán. */
export interface HoSo {
  maNguoiDung: number
  email: string
  hoTen: string
  soDienThoai: string | null
  diaChi: string | null
  maLoaiDa: number | null
  vaiTro: VaiTro
}

export interface DangNhapRequest {
  email: string
  matKhau: string
}

export interface TokenResponse {
  accessToken: string
  hetHanLuc: string
  nguoiDung: NguoiDung
}
