import { Link } from 'react-router'

export default function NotFoundPage() {
  return (
    <div className="mx-auto max-w-xl px-4 py-24 text-center">
      <p className="text-5xl font-bold text-brand-500">404</p>
      <p className="mt-4 text-gray-600">Trang bạn tìm không tồn tại.</p>
      <Link to="/" className="mt-6 inline-block text-brand-600 hover:underline">
        Về trang chủ
      </Link>
    </div>
  )
}
