import type { DanhMuc, KhiaCanh, LoaiDa, ThuongHieu } from '@/types/traCuu'
import { api } from './client'

export const traCuuApi = {
  danhMuc: () => api.get<DanhMuc[]>('/danh-muc'),
  thuongHieu: () => api.get<ThuongHieu[]>('/thuong-hieu'),
  loaiDa: () => api.get<LoaiDa[]>('/loai-da'),
  khiaCanh: () => api.get<KhiaCanh[]>('/khia-canh'),
}
