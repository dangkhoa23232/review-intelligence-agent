import type { KetQuaPhanTrang } from '@/types/common'
import type { DanhGia } from '@/types/danhGia'
import type {
  AnhDaTaiLen,
  AnhTrongThuMuc,
  DanhSachSanPhamQuanTri,
  LuuBienTheRequest,
  LuuSanPhamRequest,
  NhapLoHangRequest,
  SanPhamChiTiet,
  SanPhamQuanTriChiTiet,
  SanPhamQuanTriParams,
  SanPhamTomTat,
  TimKiemSanPhamParams,
  TrangThaiSanPham,
} from '@/types/sanPham'
import { api, taoQuery } from './client'

export const sanPhamApi = {
  timKiem: (params: TimKiemSanPhamParams, signal?: AbortSignal) =>
    api.get<KetQuaPhanTrang<SanPhamTomTat>>(`/san-pham${taoQuery(params)}`, signal),

  chiTiet: (duongDan: string, signal?: AbortSignal) =>
    api.get<SanPhamChiTiet>(`/san-pham/${encodeURIComponent(duongDan)}`, signal),

  danhGia: (duongDan: string, trang: number, kichThuoc = 10) =>
    api.get<KetQuaPhanTrang<DanhGia>>(
      `/san-pham/${encodeURIComponent(duongDan)}/danh-gia${taoQuery({ trang, kichThuoc })}`,
    ),
}

/** /api/admin/san-pham + /api/admin/upload-anh (QuanTri). */
export const adminSanPhamApi = {
  danhSach: (params: SanPhamQuanTriParams, signal?: AbortSignal) =>
    api.get<DanhSachSanPhamQuanTri>(`/admin/san-pham${taoQuery(params)}`, signal),

  chiTiet: (ma: number, signal?: AbortSignal) => api.get<SanPhamQuanTriChiTiet>(`/admin/san-pham/${ma}`, signal),

  tao: (req: LuuSanPhamRequest) => api.post<SanPhamQuanTriChiTiet>('/admin/san-pham', req),

  capNhat: (ma: number, req: LuuSanPhamRequest) => api.put<SanPhamQuanTriChiTiet>(`/admin/san-pham/${ma}`, req),

  doiTrangThai: (ma: number, trangThai: TrangThaiSanPham) =>
    api.put<SanPhamQuanTriChiTiet>(`/admin/san-pham/${ma}/trang-thai`, { trangThai }),

  themBienThe: (ma: number, req: LuuBienTheRequest) =>
    api.post<SanPhamQuanTriChiTiet>(`/admin/san-pham/${ma}/bien-the`, req),

  capNhatBienThe: (ma: number, maBienThe: number, req: LuuBienTheRequest) =>
    api.put<SanPhamQuanTriChiTiet>(`/admin/san-pham/${ma}/bien-the/${maBienThe}`, req),

  nhapLoHang: (ma: number, maBienThe: number, req: NhapLoHangRequest) =>
    api.post<SanPhamQuanTriChiTiet>(`/admin/san-pham/${ma}/bien-the/${maBienThe}/lo-hang`, req),

  anhTrongThuMuc: (signal?: AbortSignal) => api.get<AnhTrongThuMuc[]>('/admin/anh-san-pham', signal),

  taiAnh: (tep: File) => {
    const form = new FormData()
    form.append('tep', tep)
    return api.postForm<AnhDaTaiLen>('/admin/upload-anh', form)
  },
}
