import type { ReactNode } from 'react'
import { createBrowserRouter } from 'react-router'
import RequireAuth from '@/components/auth/RequireAuth'
import MainLayout from '@/components/layout/MainLayout'
import AdminOrdersPage from '@/pages/admin/AdminOrdersPage'
import AdminReviewsPage from '@/pages/admin/AdminReviewsPage'
import CartPage from '@/pages/CartPage'
import CheckoutPage from '@/pages/CheckoutPage'
import HomePage from '@/pages/HomePage'
import LoginPage from '@/pages/LoginPage'
import NotFoundPage from '@/pages/NotFoundPage'
import OrderDetailPage from '@/pages/OrderDetailPage'
import OrdersPage from '@/pages/OrdersPage'
import ProductDetailPage from '@/pages/ProductDetailPage'
import ProductListPage from '@/pages/ProductListPage'

const khachHang = (el: ReactNode) => <RequireAuth vaiTro="KhachHang">{el}</RequireAuth>
const quanTri = (el: ReactNode) => <RequireAuth vaiTro="QuanTri">{el}</RequireAuth>

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'dang-nhap', element: <LoginPage /> },
      { path: 'san-pham', element: <ProductListPage /> },
      { path: 'san-pham/:duongDan', element: <ProductDetailPage /> },

      // Khách hàng
      { path: 'gio-hang', element: khachHang(<CartPage />) },
      { path: 'thanh-toan', element: khachHang(<CheckoutPage />) },
      { path: 'don-hang', element: khachHang(<OrdersPage />) },
      { path: 'don-hang/:id', element: khachHang(<OrderDetailPage />) },

      // Quản trị
      { path: 'admin/danh-gia', element: quanTri(<AdminReviewsPage />) },
      { path: 'admin/don-hang', element: quanTri(<AdminOrdersPage />) },

      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
