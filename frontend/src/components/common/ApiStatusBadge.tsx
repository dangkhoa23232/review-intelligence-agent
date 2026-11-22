import { useEffect, useState } from 'react'
import { healthApi } from '@/services/api/healthApi'

type Status = 'dang-kiem-tra' | 'ok' | 'loi'

const styles: Record<Status, string> = {
  'dang-kiem-tra': 'bg-gray-100 text-gray-600',
  ok: 'bg-green-100 text-green-700',
  loi: 'bg-red-100 text-red-700',
}

const labels: Record<Status, string> = {
  'dang-kiem-tra': 'Đang kiểm tra…',
  ok: 'Đã kết nối',
  loi: 'Không kết nối được',
}

/** Hiển thị trạng thái kết nối tới backend (GET /api/health). */
export default function ApiStatusBadge({ name, check }: { name: string; check?: () => Promise<unknown> }) {
  const [status, setStatus] = useState<Status>('dang-kiem-tra')

  useEffect(() => {
    let cancelled = false
    ;(check ?? healthApi.backend)()
      .then(() => !cancelled && setStatus('ok'))
      .catch(() => !cancelled && setStatus('loi'))
    return () => {
      cancelled = true
    }
  }, [check])

  return (
    <span className={`inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs font-medium ${styles[status]}`}>
      {name}: {labels[status]}
    </span>
  )
}
