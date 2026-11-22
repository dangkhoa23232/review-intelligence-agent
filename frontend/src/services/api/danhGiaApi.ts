import type {
  BaoCaoDanhGia,
  DanhGiaDaTao,
  DanhSachDanhGiaQuanTri,
  DoiTrangThaiDanhGiaResponse,
  DongDonCoTheDanhGia,
  TaoDanhGiaRequest,
  TrangThaiDanhGia,
} from '@/types/danhGia'
import { api, taoQuery } from './client'

export const danhGiaApi = {
  tao: (body: TaoDanhGiaRequest) => api.post<DanhGiaDaTao>('/danh-gia', body),

  dongDonCoTheDanhGia: (maSanPham: number) =>
    api.get<DongDonCoTheDanhGia[]>(`/danh-gia/dong-don-co-the-danh-gia${taoQuery({ maSanPham })}`),

  baoCao: (maDanhGia: number, lyDo: string) =>
    api.post<BaoCaoDanhGia>(`/danh-gia/${maDanhGia}/bao-cao`, { lyDo }),
}

export const adminDanhGiaApi = {
  danhSach: (params: { trangThai?: TrangThaiDanhGia; trang?: number; kichThuoc?: number }) =>
    api.get<DanhSachDanhGiaQuanTri>(`/admin/danh-gia${taoQuery(params)}`),

  doiTrangThai: (maDanhGia: number, trangThai: Extract<TrangThaiDanhGia, 'DaDuyet' | 'DaAn'>) =>
    api.put<DoiTrangThaiDanhGiaResponse>(`/admin/danh-gia/${maDanhGia}/trang-thai`, { trangThai }),
}
