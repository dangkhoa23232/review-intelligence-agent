import type { TomTatKhiaCanh } from '@/types/sanPham'

/** Tóm tắt ưu/nhược điểm theo khía cạnh (dbo.TomTatKhiaCanh). */
export default function AspectSummary({ tomTat }: { tomTat: TomTatKhiaCanh[] }) {
  if (tomTat.length === 0) {
    return (
      <p className="rounded-xl bg-gray-50 px-4 py-6 text-center text-sm text-gray-500">
        Chưa đủ đánh giá để AI tóm tắt theo khía cạnh.
      </p>
    )
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2">
      {tomTat.map((t) => {
        const tong = t.soTichCuc + t.soTieuCuc
        const tiLe = tong === 0 ? 0 : Math.round((t.soTichCuc / tong) * 100)
        return (
          <article key={t.maKhiaCanh} className="rounded-2xl border border-gray-100 p-4">
            <div className="flex items-baseline justify-between gap-2">
              <h3 className="font-semibold text-gray-900">{t.tenKhiaCanh}</h3>
              <span className="text-xs text-gray-500">
                {t.soTichCuc} tích cực · {t.soTieuCuc} tiêu cực
              </span>
            </div>

            <div
              className="mt-2 flex h-2 overflow-hidden rounded-full bg-red-200"
              role="img"
              aria-label={`${tiLe}% ý kiến tích cực`}
            >
              <div className="bg-green-500" style={{ width: `${tiLe}%` }} />
            </div>

            <dl className="mt-3 space-y-2 text-sm">
              {t.uuDiem && (
                <div>
                  <dt className="font-medium text-green-700">Ưu điểm</dt>
                  <dd className="text-gray-700">{t.uuDiem}</dd>
                </div>
              )}
              {t.nhuocDiem && (
                <div>
                  <dt className="font-medium text-red-700">Nhược điểm</dt>
                  <dd className="text-gray-700">{t.nhuocDiem}</dd>
                </div>
              )}
              {t.vanDeLapLai && (
                <div>
                  <dt className="font-medium text-amber-700">Vấn đề lặp lại</dt>
                  <dd className="text-gray-700">{t.vanDeLapLai}</dd>
                </div>
              )}
            </dl>

            {t.danChung.length > 0 && (
              <p className="mt-3 text-xs text-gray-500">
                Dẫn chứng:{' '}
                {t.danChung.map((ma, i) => (
                  <span key={ma}>
                    {i > 0 && ', '}
                    <a href={`#danh-gia-${ma}`} className="text-brand-600 hover:underline">
                      đánh giá #{ma}
                    </a>
                  </span>
                ))}
              </p>
            )}
          </article>
        )
      })}
    </div>
  )
}
