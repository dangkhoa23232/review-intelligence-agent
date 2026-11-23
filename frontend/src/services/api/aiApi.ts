import type { KetQuaPhanTrang } from '@/types/common'
import type {
  BangDieuKhien,
  ChiSoThucNghiem,
  DanhSachTrace,
  HoiDapResponse,
  KetQuaMau,
  MoHinhAbsa,
  TraceChiTiet,
  TrangThaiAi,
  TrangThaiThucNghiem,
} from '@/types/ai'
import { api, taoQuery } from './client'

export const hoiDapApi = {
  hoi: (cauHoi: string, maSanPham?: number, maPhien?: string) =>
    api.post<HoiDapResponse>('/hoi-dap', { cauHoi, maSanPham, maPhien }),
}

export const adminAiApi = {
  trangThai: (thuKhoiTao = false) => api.get<TrangThaiAi>(`/admin/ai/trang-thai${taoQuery({ thuKhoiTao })}`),
  dongBo: (tatCa = false) => api.post<{ so_danh_gia: number }>(`/admin/ai/dong-bo${taoQuery({ tatCa })}`),

  danhSachTrace: (trang: number, maSanPham?: number) =>
    api.get<DanhSachTrace>(`/admin/ai/tac-tu${taoQuery({ trang, kichThuoc: 15, maSanPham })}`),
  chiTietTrace: (maTinNhan: number) => api.get<TraceChiTiet>(`/admin/ai/tac-tu/${maTinNhan}`),

  bangDieuKhien: (maSanPham: number | undefined, soNgay: number) =>
    api.get<BangDieuKhien>(`/admin/ai/bang-dieu-khien${taoQuery({ maSanPham, soNgay })}`),

  chiSo: () => api.get<ChiSoThucNghiem>('/admin/ai/thuc-nghiem/chi-so'),
  ketQua: (moHinh: MoHinhAbsa, chiSai: boolean, trang: number) =>
    api.get<KetQuaPhanTrang<KetQuaMau>>(`/admin/ai/thuc-nghiem/ket-qua${taoQuery({ moHinh, chiSai, trang, kichThuoc: 15 })}`),
  napMau: () => api.post<{ da_them: number; bo_qua_trung: number }>('/admin/ai/thuc-nghiem/nap-mau'),
  chay: (moHinh: MoHinhAbsa[]) => api.post<{ trang_thai: string }>('/admin/ai/thuc-nghiem/chay', { moHinh }),
  trangThaiThucNghiem: () => api.get<TrangThaiThucNghiem>('/admin/ai/thuc-nghiem/trang-thai'),
}
