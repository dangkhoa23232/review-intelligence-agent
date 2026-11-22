import { useEffect, useState } from 'react'

/** Ô số lượng có nút −/+; chặn ngoài khoảng [toiThieu, toiDa]. */
export default function QuantityStepper({
  giaTri,
  toiDa,
  toiThieu = 1,
  onChange,
  disabled = false,
  nhan = 'Số lượng',
}: {
  giaTri: number
  toiDa: number
  toiThieu?: number
  onChange: (v: number) => void
  disabled?: boolean
  nhan?: string
}) {
  const [nhap, setNhap] = useState(String(giaTri))
  useEffect(() => setNhap(String(giaTri)), [giaTri])

  const chot = () => {
    const n = Math.trunc(Number(nhap))
    if (!Number.isFinite(n) || n < toiThieu) return setNhap(String(giaTri))
    const v = Math.min(n, Math.max(toiThieu, toiDa))
    setNhap(String(v))
    if (v !== giaTri) onChange(v)
  }

  const nut =
    'flex h-9 w-9 items-center justify-center text-lg text-gray-600 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-40'

  return (
    <div className="inline-flex items-center overflow-hidden rounded-lg border border-gray-300">
      <button
        type="button"
        className={nut}
        aria-label="Giảm"
        disabled={disabled || giaTri <= toiThieu}
        onClick={() => onChange(giaTri - 1)}
      >
        −
      </button>
      <input
        type="text"
        inputMode="numeric"
        aria-label={nhan}
        value={nhap}
        disabled={disabled}
        onChange={(e) => setNhap(e.target.value.replace(/\D/g, ''))}
        onBlur={chot}
        onKeyDown={(e) => e.key === 'Enter' && chot()}
        className="h-9 w-12 border-x border-gray-300 text-center text-sm focus:outline-none disabled:bg-gray-50"
      />
      <button
        type="button"
        className={nut}
        aria-label="Tăng"
        disabled={disabled || giaTri >= toiDa}
        onClick={() => onChange(giaTri + 1)}
      >
        +
      </button>
    </div>
  )
}
