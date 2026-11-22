import { useCallback, useEffect, useState, type DependencyList } from 'react'
import { ApiError } from '@/services/api/client'

export interface AsyncState<T> {
  duLieu: T | null
  dangTai: boolean
  loi: string | null
  taiLai: () => void
}

/** Gọi hàm async khi deps đổi; tự hủy kết quả cũ (AbortController) để tránh race condition. */
export function useAsync<T>(fn: (signal: AbortSignal) => Promise<T>, deps: DependencyList): AsyncState<T> {
  const [duLieu, setDuLieu] = useState<T | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [loi, setLoi] = useState<string | null>(null)
  const [lan, setLan] = useState(0)

  useEffect(() => {
    const ctrl = new AbortController()
    setDangTai(true)
    setLoi(null)
    fn(ctrl.signal)
      .then((d) => !ctrl.signal.aborted && setDuLieu(d))
      .catch((e: unknown) => {
        if (ctrl.signal.aborted) return
        setLoi(e instanceof ApiError ? e.message : 'Không thể kết nối máy chủ.')
      })
      .finally(() => !ctrl.signal.aborted && setDangTai(false))
    return () => ctrl.abort()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, lan])

  const taiLai = useCallback(() => setLan((x) => x + 1), [])
  return { duLieu, dangTai, loi, taiLai }
}

export function thongBaoLoi(e: unknown): string {
  return e instanceof ApiError ? e.message : 'Không thể kết nối máy chủ.'
}
