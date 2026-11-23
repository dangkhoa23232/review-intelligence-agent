import { useEffect, useRef, useState, type FormEvent } from 'react'
import { hoiDapApi } from '@/services/api/aiApi'
import type { DanChung, HoiDapResponse } from '@/types/ai'
import { thongBaoLoi } from '@/utils/useAsync'
import AgentTrace from './AgentTrace'

type Tin = { loai: 'hoi'; noiDung: string } | { loai: 'tra_loi'; kq: HoiDapResponse } | { loai: 'loi'; noiDung: string }

const GOI_Y = ['Sản phẩm này có gây kích ứng không?', 'Thấm có nhanh, có bết không?', 'Mùi hương thế nào?', 'Có đáng tiền không?']

/** Thay [n] trong câu trả lời bằng nút trích dẫn trỏ tới đánh giá tương ứng. */
function NoiDungTraLoi({ text, danChung }: { text: string; danChung: DanChung[] }) {
  const phan = text.split(/(\[\d+\])/g)
  return (
    <p className="whitespace-pre-line">
      {phan.map((p, i) => {
        const m = /^\[(\d+)\]$/.exec(p)
        const dc = m ? danChung[Number(m[1]) - 1] : undefined
        return dc ? (
          <a
            key={i}
            href={`#danh-gia-${dc.maDanhGia}`}
            title={dc.doanTrich}
            className="mx-0.5 rounded bg-brand-100 px-1 text-[11px] font-semibold text-brand-700 hover:bg-brand-500 hover:text-white"
          >
            {m![1]}
          </a>
        ) : (
          <span key={i}>{p}</span>
        )
      })}
    </p>
  )
}

function TraLoi({ kq }: { kq: HoiDapResponse }) {
  const [moTrace, setMoTrace] = useState(false)
  return (
    <div className="max-w-[90%] rounded-2xl rounded-tl-sm bg-gray-50 px-4 py-3 text-sm text-gray-800">
      <NoiDungTraLoi text={kq.traLoi} danChung={kq.danChung} />

      {kq.danChung.length > 0 && (
        <ol className="mt-3 space-y-2 border-t border-gray-200 pt-3">
          {kq.danChung.map((d, i) => (
            <li key={d.maDanhGia} className="text-xs text-gray-600">
              <a href={`#danh-gia-${d.maDanhGia}`} className="font-semibold text-brand-700 hover:underline">
                [{i + 1}]
              </a>{' '}
              “{d.doanTrich}” — {d.tenNguoiDanhGia}, {d.soSao}★{d.daMuaHang ? ' · ✓ Đã mua hàng' : ''}
            </li>
          ))}
        </ol>
      )}

      <div className="mt-2 flex flex-wrap items-center gap-3 text-[11px] text-gray-400">
        <span>{kq.sinhBang === 'llm' ? 'Sinh bởi Claude' : 'Tổng hợp từ dẫn chứng'}</span>
        <span>{(kq.doTreMs / 1000).toFixed(1)} s</span>
        <button type="button" onClick={() => setMoTrace((x) => !x)} className="underline underline-offset-2 hover:text-gray-600">
          {moTrace ? 'Ẩn các bước tác tử' : `Xem ${kq.buoc.length} bước tác tử`}
        </button>
      </div>
      {moTrace && (
        <div className="mt-3 rounded-xl bg-white p-3">
          <AgentTrace buoc={kq.buoc} gonGang />
        </div>
      )}
    </div>
  )
}

/** Hỏi đáp RAG về sản phẩm – trả lời chỉ dựa trên đánh giá đã duyệt, kèm trích dẫn. */
export default function ChatBox({ maSanPham, tenSanPham }: { maSanPham: number; tenSanPham: string }) {
  const [tin, setTin] = useState<Tin[]>([])
  const [cauHoi, setCauHoi] = useState('')
  const [dangHoi, setDangHoi] = useState(false)
  const cuoi = useRef<HTMLDivElement>(null)
  const maPhien = useRef<string | undefined>(undefined)

  // Mỗi sản phẩm một phiên hội thoại mới (TinNhan.MaPhien) – lịch sử xem lại ở trang quản trị tác tử.
  useEffect(() => {
    maPhien.current = undefined
    setTin([])
  }, [maSanPham])

  useEffect(() => {
    cuoi.current?.scrollIntoView({ behavior: 'smooth', block: 'nearest' })
  }, [tin, dangHoi])

  async function gui(noiDung: string) {
    const q = noiDung.trim()
    if (q.length < 2 || dangHoi) return
    setTin((t) => [...t, { loai: 'hoi', noiDung: q }])
    setCauHoi('')
    setDangHoi(true)
    try {
      const kq = await hoiDapApi.hoi(q, maSanPham, maPhien.current)
      maPhien.current = kq.maPhien
      setTin((t) => [...t, { loai: 'tra_loi', kq }])
    } catch (err) {
      setTin((t) => [...t, { loai: 'loi', noiDung: thongBaoLoi(err) }])
    } finally {
      setDangHoi(false)
    }
  }

  function onSubmit(e: FormEvent) {
    e.preventDefault()
    void gui(cauHoi)
  }

  return (
    <div className="flex flex-col rounded-2xl border border-gray-100 shadow-sm">
      <div className="border-b border-gray-100 px-5 py-3">
        <p className="font-semibold text-gray-900">Hỏi trợ lý AI về “{tenSanPham}”</p>
        <p className="text-xs text-gray-500">Trả lời dựa trên đánh giá đã duyệt của khách hàng, kèm trích dẫn.</p>
      </div>

      <div className="max-h-[28rem] min-h-40 space-y-3 overflow-y-auto px-5 py-4" aria-live="polite">
        {tin.length === 0 && (
          <div className="flex flex-wrap gap-2">
            {GOI_Y.map((g) => (
              <button
                key={g}
                type="button"
                onClick={() => void gui(g)}
                className="rounded-full border border-gray-200 px-3 py-1.5 text-xs text-gray-600 hover:border-brand-300 hover:text-brand-700"
              >
                {g}
              </button>
            ))}
          </div>
        )}
        {tin.map((t, i) =>
          t.loai === 'hoi' ? (
            <div key={i} className="flex justify-end">
              <p className="max-w-[85%] rounded-2xl rounded-tr-sm bg-brand-500 px-4 py-2 text-sm text-white">{t.noiDung}</p>
            </div>
          ) : t.loai === 'tra_loi' ? (
            <TraLoi key={i} kq={t.kq} />
          ) : (
            <p key={i} className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
              {t.noiDung}
            </p>
          ),
        )}
        {dangHoi && (
          <p className="flex items-center gap-2 text-xs text-gray-500" role="status">
            <span className="h-3 w-3 animate-spin rounded-full border-2 border-brand-500 border-t-transparent" />
            Trợ lý đang tra cứu đánh giá…
          </p>
        )}
        <div ref={cuoi} />
      </div>

      <form onSubmit={onSubmit} className="flex gap-2 border-t border-gray-100 p-3">
        <input
          value={cauHoi}
          onChange={(e) => setCauHoi(e.target.value)}
          maxLength={500}
          placeholder="Ví dụ: Da nhạy cảm dùng có bị châm chích không?"
          aria-label="Câu hỏi cho trợ lý"
          className="min-w-0 flex-1 rounded-full border border-gray-300 px-4 py-2 text-sm focus:border-brand-500 focus:outline-none"
        />
        <button
          type="submit"
          disabled={dangHoi || cauHoi.trim().length < 2}
          className="rounded-full bg-brand-500 px-5 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-50"
        >
          Hỏi
        </button>
      </form>
    </div>
  )
}
