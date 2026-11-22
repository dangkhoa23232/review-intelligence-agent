import { Link, NavLink } from 'react-router'

const navItems = [
  { to: '/', label: 'Trang chủ' },
  { to: '/san-pham', label: 'Sản phẩm' },
]

export default function Header() {
  return (
    <header className="border-b border-gray-100 bg-white/90 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4">
        <Link to="/" className="text-lg font-bold text-brand-600">
          Review Intelligence
        </Link>

        <nav className="flex items-center gap-6 text-sm">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end
              className={({ isActive }) =>
                isActive ? 'font-semibold text-brand-600' : 'text-gray-600 hover:text-brand-600'
              }
            >
              {item.label}
            </NavLink>
          ))}
          <Link
            to="/dang-nhap"
            className="rounded-full bg-brand-500 px-4 py-1.5 font-medium text-white hover:bg-brand-600"
          >
            Đăng nhập
          </Link>
        </nav>
      </div>
    </header>
  )
}
