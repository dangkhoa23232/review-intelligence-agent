import type { DangNhapRequest, NguoiDung, TokenResponse } from '@/types/auth'
import { api, tokenStorage } from './client'

export const authApi = {
  async dangNhap(body: DangNhapRequest): Promise<TokenResponse> {
    const res = await api.post<TokenResponse>('/auth/dang-nhap', body)
    tokenStorage.set(res.accessToken)
    return res
  },
  toi: () => api.get<Pick<NguoiDung, 'maNguoiDung' | 'email' | 'hoTen' | 'vaiTro'>>('/auth/toi'),
  dangXuat: () => tokenStorage.clear(),
}
