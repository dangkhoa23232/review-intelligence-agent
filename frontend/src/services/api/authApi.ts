import type { DangNhapRequest, NguoiDung, TokenResponse } from '@/types/auth'
import { api, tokenStorage, userStorage } from './client'

export const authApi = {
  async dangNhap(body: DangNhapRequest): Promise<TokenResponse> {
    const res = await api.post<TokenResponse>('/auth/dang-nhap', body)
    tokenStorage.set(res.accessToken)
    userStorage.set(res.nguoiDung)
    return res
  },
  toi: () => api.get<Pick<NguoiDung, 'maNguoiDung' | 'email' | 'hoTen' | 'vaiTro'>>('/auth/toi'),
  dangXuat: () => {
    tokenStorage.clear()
    userStorage.clear()
  },
}
