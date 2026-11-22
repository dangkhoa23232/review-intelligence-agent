import type { DangNhapRequest, HoSo, TokenResponse } from '@/types/auth'
import { api, tokenStorage, userStorage } from './client'

export const authApi = {
  async dangNhap(body: DangNhapRequest): Promise<TokenResponse> {
    const res = await api.post<TokenResponse>('/auth/dang-nhap', body)
    tokenStorage.set(res.accessToken)
    userStorage.set(res.nguoiDung)
    return res
  },
  /** Hồ sơ đầy đủ (họ tên, SĐT, địa chỉ) đọc từ DB. */
  toi: () => api.get<HoSo>('/auth/toi'),
  dangXuat: () => {
    tokenStorage.clear()
    userStorage.clear()
  },
}
