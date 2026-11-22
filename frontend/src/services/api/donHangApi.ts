import type { KetQuaPhanTrang } from '@/types/common'
import type {
  DanhSachDonHangQuanTri,
  DatHangRequest,
  DonHangChiTiet,
  DonHangTomTat,
  KiemTraPhieuGiamGia,
  TrangThaiDonHang,
} from '@/types/donHang'
import { api, taoQuery } from './client'

export const phieuGiamGiaApi = {
  kiemTra: (maCode: string, tienHang: number) =>
    api.post<KiemTraPhieuGiamGia>('/phieu-giam-gia/kiem-tra', { maCode, tienHang }),
}

export const donHangApi = {
  datHang: (body: DatHangRequest) => api.post<DonHangChiTiet>('/don-hang', body),
  danhSach: (trang: number, kichThuoc = 10) =>
    api.get<KetQuaPhanTrang<DonHangTomTat>>(`/don-hang${taoQuery({ trang, kichThuoc })}`),
  chiTiet: (maDonHang: number, signal?: AbortSignal) => api.get<DonHangChiTiet>(`/don-hang/${maDonHang}`, signal),
  huy: (maDonHang: number) => api.put<DonHangChiTiet>(`/don-hang/${maDonHang}/huy`),
}

export const adminDonHangApi = {
  danhSach: (params: { trangThai?: TrangThaiDonHang; trang?: number; kichThuoc?: number }) =>
    api.get<DanhSachDonHangQuanTri>(`/admin/don-hang${taoQuery(params)}`),
  doiTrangThai: (maDonHang: number, trangThai: TrangThaiDonHang) =>
    api.put<DonHangChiTiet>(`/admin/don-hang/${maDonHang}/trang-thai`, { trangThai }),
}
