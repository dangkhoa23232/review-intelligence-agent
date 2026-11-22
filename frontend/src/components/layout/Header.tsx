import { Link, NavLink } from 'react-router'
import { useAuth } from '@/context/useAuth'

const lopNav = ({ isActive }: { isActive: boolean }) =>
  isActive ? 'font-semibold text-brand-600' : 'text-gray-600 hover:text-brand-600'

export default function Header() {
  const { nguoiDung, dangXuat } = useAuth()

  return (
    <header className="sticky top-0 z-20 border-b border-gray-100 bg-white/90 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-4 px-4">
        <Link to="/" className="text-lg font-bold text-brand-600">
          Review Intelligence
        </Link>

        <nav className="flex items-center gap-5 text-sm">
          <NavLink to="/" end className={lopNav}>
            Trang chủ
          </NavLink>
          <NavLink to="/san-pham" className={lopNav}>
            Sản phẩm
          </NavLink>
          {nguoiDung?.vaiTro === 'QuanTri' && (
            <NavLink to="/admin/danh-gia" className={lopNav}>
              Duyệt đánh giá
            </NavLink>
          )}

          {nguoiDung ? (
            <div className="flex items-center gap-3">
              <span className="hidden text-gray-700 sm:inline">
                Xin chào, <span className="font-medium">{nguoiDung.hoTen}</span>
              </span>
              <button
                type="button"
                onClick={dangXuat}
                className="rounded-full border border-gray-300 px-4 py-1.5 font-medium text-gray-700 hover:bg-gray-50"
              >
                Đăng xuất
              </button>
            </div>
          ) : (
            <Link
              to="/dang-nhap"
              className="rounded-full bg-brand-500 px-4 py-1.5 font-medium text-white hover:bg-brand-600"
            >
              Đăng nhập
            </Link>
          )}
        </nav>
      </div>
    </header>
  )
}
