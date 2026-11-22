import { createBrowserRouter } from 'react-router'
import RequireAuth from '@/components/auth/RequireAuth'
import MainLayout from '@/components/layout/MainLayout'
import AdminReviewsPage from '@/pages/admin/AdminReviewsPage'
import HomePage from '@/pages/HomePage'
import LoginPage from '@/pages/LoginPage'
import NotFoundPage from '@/pages/NotFoundPage'
import ProductDetailPage from '@/pages/ProductDetailPage'
import ProductListPage from '@/pages/ProductListPage'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'dang-nhap', element: <LoginPage /> },
      { path: 'san-pham', element: <ProductListPage /> },
      { path: 'san-pham/:duongDan', element: <ProductDetailPage /> },
      {
        path: 'admin/danh-gia',
        element: (
          <RequireAuth vaiTro="QuanTri">
            <AdminReviewsPage />
          </RequireAuth>
        ),
      },
      // TODO: gio-hang, don-hang, ...
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
