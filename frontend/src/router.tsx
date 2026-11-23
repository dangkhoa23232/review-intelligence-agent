import type { ReactNode } from 'react'
import { Navigate, createBrowserRouter } from 'react-router'
import RequireAuth from '@/components/auth/RequireAuth'
import AdminLayout from '@/components/layout/AdminLayout'
import MainLayout from '@/components/layout/MainLayout'
import AdminAgentPage from '@/pages/admin/AdminAgentPage'
import AdminDashboardPage from '@/pages/admin/AdminDashboardPage'
import AdminExperimentPage from '@/pages/admin/AdminExperimentPage'
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

      // Quản trị (AdminLayout kiểm tra vai trò QuanTri)
      {
        path: 'admin',
        element: <AdminLayout />,
        children: [
          { index: true, element: <Navigate to="/admin/don-hang" replace /> },
          { path: 'don-hang', element: <AdminOrdersPage /> },
          { path: 'danh-gia', element: <AdminReviewsPage /> },
          { path: 'thong-ke', element: <AdminDashboardPage /> },
          { path: 'tac-tu', element: <AdminAgentPage /> },
          { path: 'thuc-nghiem', element: <AdminExperimentPage /> },
        ],
      },

      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
