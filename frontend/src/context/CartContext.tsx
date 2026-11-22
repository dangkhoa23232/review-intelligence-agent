import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { gioHangApi } from '@/services/api/gioHangApi'
import type { GioHang } from '@/types/gioHang'
import { useAuth } from './useAuth'

export interface CartState {
  /** null khi chưa tải hoặc người dùng không phải khách hàng. */
  gioHang: GioHang | null
  /** Tổng số lượng cho badge trên Header. */
  soLuong: number
  /** Cập nhật từ response của mọi API giỏ hàng (đều trả về giỏ mới nhất). */
  datGioHang: (gio: GioHang) => void
  taiLai: () => Promise<void>
}

export const CartContext = createContext<CartState | null>(null)

export function CartProvider({ children }: { children: ReactNode }) {
  const { nguoiDung } = useAuth()
  const laKhachHang = nguoiDung?.vaiTro === 'KhachHang'
  const [gioHang, setGioHang] = useState<GioHang | null>(null)

  const taiLai = useCallback(async () => {
    if (!laKhachHang) {
      setGioHang(null)
      return
    }
    try {
      setGioHang(await gioHangApi.lay())
    } catch {
      /* badge giữ nguyên nếu lỗi mạng */
    }
  }, [laKhachHang])

  useEffect(() => {
    void taiLai()
  }, [taiLai, nguoiDung?.maNguoiDung])

  const value = useMemo<CartState>(
    () => ({ gioHang, soLuong: gioHang?.tongSoLuong ?? 0, datGioHang: setGioHang, taiLai }),
    [gioHang, taiLai],
  )
  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}
