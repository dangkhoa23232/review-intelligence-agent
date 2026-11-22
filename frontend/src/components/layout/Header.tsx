import { Link, NavLink } from 'react-router'
import { useAuth } from '@/context/useAuth'
import { useCart } from '@/context/useCart'

const lopNav = ({ isActive }: { isActive: boolean }) =>
  isActive ? 'font-semibold text-brand-600' : 'text-gray-600 hover:text-brand-600'

function NutGioHang() {
  const { soLuong } = useCart()
  return (
    <Link
      to="/gio-hang"
      className="relative rounded-full p-2 text-gray-600 hover:bg-gray-100 hover:text-brand-600"
      aria-label={soLuong > 0 ? `Giỏ hàng, ${soLuong} sản phẩm` : 'Giỏ hàng'}
    >
      <svg viewBox="0 0 24 24" className="h-6 w-6" fill="none" stroke="currentColor" strokeWidth={1.8} aria-hidden>
        <path strokeLinecap="round" strokeLinejoin="round" d="M3 4h2l2.4 11.2a2 2 0 0 0 2 1.6h7.7a2 2 0 0 0 2-1.5L21 8H6.2" />
        <circle cx="10" cy="20" r="1.3" />
        <circle cx="17" cy="20" r="1.3" />
      </svg>
      {soLuong > 0 && (
        <span className="absolute -right-0.5 -top-0.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-brand-500 px-1 text-[11px] font-semibold text-white">
          {soLuong > 99 ? '99+' : soLuong}
        </span>
      )}
    </Link>
  )
}

export default function Header() {
  const { nguoiDung, dangXuat } = useAuth()
  const laQuanTri = nguoiDung?.vaiTro === 'QuanTri'

  return (
    <header className="sticky top-0 z-20 border-b border-gray-100 bg-white/90 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-4 px-4">
        <Link to="/" className="text-lg font-bold text-brand-600">
          Review Intelligence
        </Link>

        <nav className="flex items-center gap-4 text-sm sm:gap-5">
          <NavLink to="/" end className={({ isActive }) => `hidden sm:inline ${lopNav({ isActive })}`}>
            Trang chủ
          </NavLink>
          <NavLink to="/san-pham" className={lopNav}>
            Sản phẩm
          </NavLink>
          {nguoiDung && !laQuanTri && (
            <NavLink to="/don-hang" className={lopNav}>
              Đơn hàng
            </NavLink>
          )}
          {laQuanTri && (
            <>
              <NavLink to="/admin/don-hang" className={lopNav}>
                Quản lý đơn
              </NavLink>
              <NavLink to="/admin/danh-gia" className={lopNav}>
                Duyệt đánh giá
              </NavLink>
            </>
          )}

          {!laQuanTri && <NutGioHang />}

          {nguoiDung ? (
            <div className="flex items-center gap-3">
              <span className="hidden text-gray-700 lg:inline">
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
