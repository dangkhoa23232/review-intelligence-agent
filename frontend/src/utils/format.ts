import type { CamXuc, TrangThaiDanhGia } from '@/types/danhGia'

const tienVnd = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })
const ngay = new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
const ngayGio = new Intl.DateTimeFormat('vi-VN', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

export const dinhDangTien = (v: number) => tienVnd.format(v)
export const dinhDangNgay = (iso: string) => ngay.format(new Date(iso))
export const dinhDangNgayGio = (iso: string) => ngayGio.format(new Date(iso))

export const NHAN_CAM_XUC: Record<CamXuc, string> = {
  TichCuc: 'Tích cực',
  TrungTinh: 'Trung tính',
  TieuCuc: 'Tiêu cực',
}

export const MAU_CAM_XUC: Record<CamXuc, string> = {
  TichCuc: 'bg-green-50 text-green-700 ring-green-600/20',
  TrungTinh: 'bg-gray-50 text-gray-700 ring-gray-500/20',
  TieuCuc: 'bg-red-50 text-red-700 ring-red-600/20',
}

export const NHAN_TRANG_THAI_DANH_GIA: Record<TrangThaiDanhGia, string> = {
  ChoDuyet: 'Chờ duyệt',
  DaDuyet: 'Đã duyệt',
  DaAn: 'Đã ẩn',
}
