import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '@/services/api/authApi'
import { SU_KIEN_HET_PHIEN, userStorage } from '@/services/api/client'
import type { NguoiDung } from '@/types/auth'

export interface AuthState {
  nguoiDung: NguoiDung | null
  dangNhap: (email: string, matKhau: string) => Promise<NguoiDung>
  dangXuat: () => void
}

export const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [nguoiDung, setNguoiDung] = useState<NguoiDung | null>(() => userStorage.get())

  useEffect(() => {
    const onHetPhien = () => setNguoiDung(null)
    window.addEventListener(SU_KIEN_HET_PHIEN, onHetPhien)
    return () => window.removeEventListener(SU_KIEN_HET_PHIEN, onHetPhien)
  }, [])

  const dangNhap = useCallback(async (email: string, matKhau: string) => {
    const res = await authApi.dangNhap({ email, matKhau })
    setNguoiDung(res.nguoiDung)
    return res.nguoiDung
  }, [])

  const dangXuat = useCallback(() => {
    authApi.dangXuat()
    setNguoiDung(null)
  }, [])

  const value = useMemo(() => ({ nguoiDung, dangNhap, dangXuat }), [nguoiDung, dangNhap, dangXuat])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
