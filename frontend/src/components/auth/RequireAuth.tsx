import type { ReactNode } from 'react'
import { Link, Navigate, useLocation } from 'react-router'
import { useAuth } from '@/context/useAuth'
import type { VaiTro } from '@/types/auth'

/** Bảo vệ route: chưa đăng nhập → /dang-nhap?tiepTuc=...; sai vai trò → thông báo 403. */
export default function RequireAuth({ vaiTro, children }: { vaiTro?: VaiTro; children: ReactNode }) {
  const { nguoiDung } = useAuth()
  const location = useLocation()

  if (!nguoiDung) {
    const tiepTuc = encodeURIComponent(location.pathname + location.search)
    return <Navigate to={`/dang-nhap?tiepTuc=${tiepTuc}`} replace />
  }

  if (vaiTro && nguoiDung.vaiTro !== vaiTro) {
    return (
      <div className="mx-auto max-w-md px-4 py-24 text-center">
        <p className="text-5xl font-bold text-brand-500">403</p>
        <p className="mt-4 text-gray-600">Tài khoản của bạn không có quyền truy cập trang này.</p>
        <Link to="/" className="mt-6 inline-block text-brand-600 hover:underline">
          Về trang chủ
        </Link>
      </div>
    )
  }

  return <>{children}</>
}
