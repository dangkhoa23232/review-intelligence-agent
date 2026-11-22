import { useState } from 'react'

const DUONG_SAO =
  'M10 1.5l2.6 5.3 5.9.9-4.25 4.1 1 5.85L10 14.9l-5.25 2.75 1-5.85L1.5 7.7l5.9-.9L10 1.5z'

function Sao({ phan, kichThuoc }: { phan: number; kichThuoc: number }) {
  // phan: 0..1 – tỉ lệ tô màu của ngôi sao
  return (
    <span className="relative inline-block" style={{ width: kichThuoc, height: kichThuoc }}>
      <svg viewBox="0 0 20 20" className="absolute inset-0 h-full w-full fill-gray-200" aria-hidden>
        <path d={DUONG_SAO} />
      </svg>
      <span className="absolute inset-0 overflow-hidden" style={{ width: `${phan * 100}%` }}>
        <svg viewBox="0 0 20 20" className="fill-amber-400" style={{ width: kichThuoc, height: kichThuoc }} aria-hidden>
          <path d={DUONG_SAO} />
        </svg>
      </span>
    </span>
  )
}

/** Hiển thị điểm sao (hỗ trợ số lẻ, ví dụ 3.5). */
export function StarRating({ diem, kichThuoc = 16 }: { diem: number; kichThuoc?: number }) {
  return (
    <span className="inline-flex items-center gap-0.5" aria-label={`${diem.toFixed(1)} trên 5 sao`}>
      {[1, 2, 3, 4, 5].map((i) => (
        <Sao key={i} phan={Math.max(0, Math.min(1, diem - (i - 1)))} kichThuoc={kichThuoc} />
      ))}
    </span>
  )
}

/** Chọn số sao 1..5 (bàn phím: Tab tới từng sao, Enter/Space để chọn). */
export function StarInput({
  giaTri,
  onChange,
  kichThuoc = 28,
  nhan,
}: {
  giaTri: number
  onChange: (v: number) => void
  kichThuoc?: number
  nhan: string
}) {
  const [hover, setHover] = useState(0)
  const hienThi = hover || giaTri

  return (
    <div role="radiogroup" aria-label={nhan} className="inline-flex gap-1" onMouseLeave={() => setHover(0)}>
      {[1, 2, 3, 4, 5].map((i) => (
        <button
          key={i}
          type="button"
          role="radio"
          aria-checked={giaTri === i}
          aria-label={`${i} sao`}
          onMouseEnter={() => setHover(i)}
          onClick={() => onChange(i)}
          className="rounded transition-transform hover:scale-110 focus-visible:outline-2 focus-visible:outline-brand-500"
        >
          <Sao phan={i <= hienThi ? 1 : 0} kichThuoc={kichThuoc} />
        </button>
      ))}
    </div>
  )
}
