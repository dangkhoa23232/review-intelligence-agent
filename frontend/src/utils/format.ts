import type { CamXuc, TrangThaiDanhGia } from '@/types/danhGia'
import type { PhuongThucGiaoHang, PhuongThucThanhToan, TrangThaiDonHang } from '@/types/donHang'

export const NHAN_TRANG_THAI_DON: Record<TrangThaiDonHang, string> = {
  ChoXacNhan: 'Chờ xác nhận',
  DaXacNhan: 'Đã xác nhận',
  DangDongGoi: 'Đang đóng gói',
  DangGiao: 'Đang giao',
  HoanTat: 'Hoàn tất',
  DaHuy: 'Đã hủy',
}

export const MAU_TRANG_THAI_DON: Record<TrangThaiDonHang, string> = {
  ChoXacNhan: 'bg-amber-50 text-amber-700 ring-amber-600/20',
  DaXacNhan: 'bg-blue-50 text-blue-700 ring-blue-600/20',
  DangDongGoi: 'bg-indigo-50 text-indigo-700 ring-indigo-600/20',
  DangGiao: 'bg-violet-50 text-violet-700 ring-violet-600/20',
  HoanTat: 'bg-green-50 text-green-700 ring-green-600/20',
  DaHuy: 'bg-gray-100 text-gray-600 ring-gray-500/20',
}

/** Nhãn nút hành động của quản trị khi chuyển sang trạng thái tương ứng. */
export const NHAN_HANH_DONG_DON: Record<TrangThaiDonHang, string> = {
  ChoXacNhan: 'Chờ xác nhận',
  DaXacNhan: 'Xác nhận đơn',
  DangDongGoi: 'Đóng gói',
  DangGiao: 'Giao hàng',
  HoanTat: 'Hoàn tất',
  DaHuy: 'Hủy đơn',
}

export const NHAN_GIAO_HANG: Record<PhuongThucGiaoHang, string> = {
  TieuChuan: 'Giao tiêu chuẩn',
  Nhanh: 'Giao nhanh',
}

export const NHAN_THANH_TOAN: Record<PhuongThucThanhToan, string> = {
  TienMat: 'Thanh toán khi nhận hàng (tiền mặt)',
  ChuyenKhoanGiaLap: 'Chuyển khoản (giả lập)',
}

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
