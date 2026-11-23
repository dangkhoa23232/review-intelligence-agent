import { NavLink, Outlet } from 'react-router'
import RequireAuth from '@/components/auth/RequireAuth'

const TAB = [
  { to: '/admin/don-hang', nhan: 'Đơn hàng' },
  { to: '/admin/danh-gia', nhan: 'Duyệt đánh giá' },
  { to: '/admin/thong-ke', nhan: 'Thống kê cảm xúc' },
  { to: '/admin/tac-tu', nhan: 'Tác tử hỏi đáp' },
  { to: '/admin/thuc-nghiem', nhan: 'Thực nghiệm mô hình' },
]

/** Khung chung cho mọi trang quản trị (chỉ QuanTri). */
export default function AdminLayout() {
  return (
    <RequireAuth vaiTro="QuanTri">
      <div className="border-b border-gray-100 bg-gray-50/60">
        <nav className="mx-auto flex max-w-6xl gap-1 overflow-x-auto px-4" aria-label="Quản trị">
          {TAB.map((t) => (
            <NavLink
              key={t.to}
              to={t.to}
              className={({ isActive }) =>
                `shrink-0 border-b-2 px-3 py-3 text-sm font-medium transition-colors ${
                  isActive ? 'border-brand-500 text-brand-600' : 'border-transparent text-gray-500 hover:text-gray-800'
                }`
              }
            >
              {t.nhan}
            </NavLink>
          ))}
        </nav>
      </div>
      <Outlet />
    </RequireAuth>
  )
}
