import { createBrowserRouter } from 'react-router'
import MainLayout from '@/components/layout/MainLayout'
import HomePage from '@/pages/HomePage'
import LoginPage from '@/pages/LoginPage'
import NotFoundPage from '@/pages/NotFoundPage'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'dang-nhap', element: <LoginPage /> },
      // TODO: san-pham, san-pham/:duongDan, gio-hang, don-hang, quan-tri/...
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
