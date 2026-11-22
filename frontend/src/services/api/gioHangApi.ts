import type { GioHang } from '@/types/gioHang'
import { api } from './client'

export const gioHangApi = {
  lay: () => api.get<GioHang>('/gio-hang'),
  them: (maBienThe: number, soLuong: number) => api.post<GioHang>('/gio-hang', { maBienThe, soLuong }),
  capNhat: (maBienThe: number, soLuong: number) => api.put<GioHang>(`/gio-hang/${maBienThe}`, { soLuong }),
  xoaDong: (maBienThe: number) => api.delete<GioHang>(`/gio-hang/${maBienThe}`),
  xoaTatCa: () => api.delete<GioHang>('/gio-hang'),
}
