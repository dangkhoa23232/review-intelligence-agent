import type { KetQuaPhanTrang } from '@/types/common'
import type { DanhGia } from '@/types/danhGia'
import type { SanPhamChiTiet, SanPhamTomTat, TimKiemSanPhamParams } from '@/types/sanPham'
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
