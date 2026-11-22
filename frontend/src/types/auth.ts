/** Khớp bảng dbo.NguoiDung (JSON camelCase từ backend). */
export type VaiTro = 'KhachHang' | 'QuanTri'

export interface NguoiDung {
  maNguoiDung: number
  email: string
  hoTen: string
  vaiTro: VaiTro
  maLoaiDa: number | null
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
