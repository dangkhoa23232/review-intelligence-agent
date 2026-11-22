/** Trang hiển thị: 1 … (trang-1) trang (trang+1) … cuối. */
function danhSachTrang(trang: number, tong: number): (number | '…')[] {
  if (tong <= 7) return Array.from({ length: tong }, (_, i) => i + 1)
  const ketQua: (number | '…')[] = [1]
  const tu = Math.max(2, trang - 1)
  const den = Math.min(tong - 1, trang + 1)
  if (tu > 2) ketQua.push('…')
  for (let i = tu; i <= den; i++) ketQua.push(i)
  if (den < tong - 1) ketQua.push('…')
  ketQua.push(tong)
  return ketQua
}

export default function Pagination({
  trang,
  tongSoTrang,
  onChange,
}: {
  trang: number
  tongSoTrang: number
  onChange: (trang: number) => void
}) {
  if (tongSoTrang <= 1) return null

  const nut = 'min-w-9 h-9 rounded-lg px-3 text-sm font-medium transition-colors disabled:opacity-40'

  return (
    <nav className="flex flex-wrap items-center justify-center gap-1" aria-label="Phân trang">
      <button
        type="button"
        className={`${nut} text-gray-600 hover:bg-gray-100`}
        disabled={trang <= 1}
        onClick={() => onChange(trang - 1)}
      >
        ‹ Trước
      </button>
      {danhSachTrang(trang, tongSoTrang).map((p, i) =>
        p === '…' ? (
          <span key={`dot-${i}`} className="px-1 text-gray-400">
            …
          </span>
        ) : (
          <button
            key={p}
            type="button"
            aria-current={p === trang ? 'page' : undefined}
            className={`${nut} ${p === trang ? 'bg-brand-500 text-white' : 'text-gray-700 hover:bg-gray-100'}`}
            onClick={() => onChange(p)}
          >
            {p}
          </button>
        ),
      )}
      <button
        type="button"
        className={`${nut} text-gray-600 hover:bg-gray-100`}
        disabled={trang >= tongSoTrang}
        onClick={() => onChange(trang + 1)}
      >
        Sau ›
      </button>
    </nav>
  )
}
