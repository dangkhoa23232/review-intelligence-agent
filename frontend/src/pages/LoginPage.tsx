import { useState, type FormEvent } from 'react'
import { Navigate, useNavigate, useSearchParams } from 'react-router'
import { useAuth } from '@/context/useAuth'
import { thongBaoLoi } from '@/utils/useAsync'

/** Chỉ chấp nhận đường dẫn nội bộ để tránh open redirect. */
function duongDanTiepTuc(raw: string | null): string {
  return raw && raw.startsWith('/') && !raw.startsWith('//') ? raw : '/'
}

export default function LoginPage() {
  const { nguoiDung, dangNhap } = useAuth()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const tiepTuc = duongDanTiepTuc(searchParams.get('tiepTuc'))

  const [email, setEmail] = useState('')
  const [matKhau, setMatKhau] = useState('')
  const [loi, setLoi] = useState<string | null>(null)
  const [dangGui, setDangGui] = useState(false)

  if (nguoiDung) return <Navigate to={tiepTuc} replace />

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setLoi(null)
    setDangGui(true)
    try {
      const nd = await dangNhap(email, matKhau)
      navigate(tiepTuc === '/' && nd.vaiTro === 'QuanTri' ? '/admin/danh-gia' : tiepTuc, { replace: true })
    } catch (err) {
      setLoi(thongBaoLoi(err))
    } finally {
      setDangGui(false)
    }
  }

  return (
    <div className="mx-auto max-w-sm px-4 py-16">
      <h1 className="text-2xl font-bold text-gray-900">Đăng nhập</h1>
      <form onSubmit={onSubmit} className="mt-6 space-y-4">
        <label className="block">
          <span className="text-sm text-gray-700">Email</span>
          <input
            type="email"
            required
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 focus:border-brand-500 focus:outline-none"
          />
        </label>
        <label className="block">
          <span className="text-sm text-gray-700">Mật khẩu</span>
          <input
            type="password"
            required
            minLength={6}
            autoComplete="current-password"
            value={matKhau}
            onChange={(e) => setMatKhau(e.target.value)}
            className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 focus:border-brand-500 focus:outline-none"
          />
        </label>
        {loi && (
          <p className="text-sm text-red-600" role="alert">
            {loi}
          </p>
        )}
        <button
          type="submit"
          disabled={dangGui}
          className="w-full rounded-lg bg-brand-500 py-2 font-medium text-white hover:bg-brand-600 disabled:opacity-60"
        >
          {dangGui ? 'Đang đăng nhập…' : 'Đăng nhập'}
        </button>
      </form>
    </div>
  )
}
