import { useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import AgentTrace from '@/components/ai/AgentTrace'
import Pagination from '@/components/common/Pagination'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import { adminAiApi } from '@/services/api/aiApi'
import { dinhDangNgayGio } from '@/utils/format'
import { useAsync } from '@/utils/useAsync'

function ChiTiet({ maTinNhan, onDong }: { maTinNhan: number; onDong: () => void }) {
  const { duLieu, dangTai, loi } = useAsync(() => adminAiApi.chiTietTrace(maTinNhan), [maTinNhan])
  return (
    <aside className="rounded-2xl border border-gray-100 bg-white p-5 lg:sticky lg:top-24 lg:max-h-[calc(100vh-7rem)] lg:overflow-y-auto">
      <div className="flex items-start justify-between gap-3">
        <h2 className="font-semibold text-gray-900">Trace tin nhắn #{maTinNhan}</h2>
        <button type="button" onClick={onDong} className="text-sm text-gray-400 hover:text-gray-700" aria-label="Đóng">
          ✕
        </button>
      </div>
      {loi ? (
        <p className="mt-3 text-sm text-red-600">{loi}</p>
      ) : dangTai || !duLieu ? (
        <DangTai />
      ) : (
        <div className="mt-3 space-y-4 text-sm">
          <div>
            <p className="text-xs font-medium uppercase tracking-wide text-gray-400">Câu hỏi</p>
            <p className="mt-1 text-gray-900">{duLieu.tinNhan.cauHoi ?? '—'}</p>
          </div>
          <div>
            <p className="text-xs font-medium uppercase tracking-wide text-gray-400">Câu trả lời</p>
            <p className="mt-1 whitespace-pre-line text-gray-800">{duLieu.tinNhan.traLoi}</p>
          </div>
          {duLieu.danChung && duLieu.danChung.length > 0 && (
            <div>
              <p className="text-xs font-medium uppercase tracking-wide text-gray-400">Dẫn chứng (TinNhan.DanChung)</p>
              <ol className="mt-1 space-y-1 text-xs text-gray-600">
                {duLieu.danChung.map((d, i) => (
                  <li key={d.maDanhGia}>
                    [{i + 1}] DanhGia#{d.maDanhGia} · điểm {d.diem} – “{d.doanTrich}”
                  </li>
                ))}
              </ol>
            </div>
          )}
          <div>
            <p className="mb-2 text-xs font-medium uppercase tracking-wide text-gray-400">
              Các bước tác tử (NhatKyTacTu) · tổng {duLieu.tinNhan.doTreMs ?? '–'} ms
            </p>
            <AgentTrace buoc={duLieu.buoc} />
          </div>
        </div>
      )}
    </aside>
  )
}

export default function AdminAgentPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const trang = Math.max(1, Number(searchParams.get('trang')) || 1)
  const [chon, setChon] = useState<number | null>(null)
  const { duLieu, dangTai, loi, taiLai } = useAsync(() => adminAiApi.danhSachTrace(trang), [trang])

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <div className="flex flex-wrap items-baseline justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Tác tử hỏi đáp</h1>
          <p className="mt-1 text-sm text-gray-500">
            Lịch sử câu hỏi (<code>TinNhan</code>) và từng bước lập kế hoạch → gọi công cụ → kiểm soát → trả lời (
            <code>NhatKyTacTu</code>).
          </p>
        </div>
        <button type="button" onClick={taiLai} className="text-sm text-brand-600 hover:underline">
          Làm mới
        </button>
      </div>

      <div className={`mt-6 grid gap-6 ${chon ? 'lg:grid-cols-[1fr_460px]' : ''}`}>
        <div>
          {loi ? (
            <HopLoi thongBao={loi} onThuLai={taiLai} />
          ) : dangTai && !duLieu ? (
            <DangTai />
          ) : duLieu && duLieu.duLieu.length === 0 ? (
            <p className="rounded-2xl bg-gray-50 px-4 py-16 text-center text-sm text-gray-500">
              Chưa có câu hỏi nào. Hãy thử hỏi trợ lý trên trang chi tiết sản phẩm.
            </p>
          ) : (
            duLieu && (
              <>
                <ul className="divide-y divide-gray-100 rounded-2xl border border-gray-100 bg-white">
                  {duLieu.duLieu.map((t) => (
                    <li key={t.maTinNhan}>
                      <button
                        type="button"
                        onClick={() => setChon(t.maTinNhan)}
                        className={`w-full px-4 py-3 text-left hover:bg-gray-50 ${chon === t.maTinNhan ? 'bg-brand-50' : ''}`}
                      >
                        <div className="flex flex-wrap items-center gap-2 text-xs text-gray-500">
                          <span>{dinhDangNgayGio(t.ngayTao)}</span>
                          {t.tenSanPham && (
                            <Link
                              to={`/san-pham?tuKhoa=${encodeURIComponent(t.tenSanPham)}`}
                              onClick={(e) => e.stopPropagation()}
                              className="text-brand-600 hover:underline"
                            >
                              {t.tenSanPham}
                            </Link>
                          )}
                          <span>· {t.tenNguoiDung ?? 'Khách vãng lai'}</span>
                          <span>· {t.soBuoc} bước</span>
                          <span>· {t.soDanChung} dẫn chứng</span>
                          {t.doTreMs !== null && <span>· {t.doTreMs} ms</span>}
                          {t.coLoi && <span className="rounded bg-red-50 px-1.5 text-red-700">Lỗi</span>}
                        </div>
                        <p className="mt-1 line-clamp-1 text-sm font-medium text-gray-900">{t.cauHoi ?? '—'}</p>
                        <p className="line-clamp-2 text-sm text-gray-600">{t.traLoi}</p>
                      </button>
                    </li>
                  ))}
                </ul>
                <div className="mt-6">
                  <Pagination
                    trang={duLieu.trang}
                    tongSoTrang={duLieu.tongSoTrang}
                    onChange={(p) => setSearchParams({ trang: String(p) })}
                  />
                </div>
              </>
            )
          )}
        </div>
        {chon && <ChiTiet maTinNhan={chon} onDong={() => setChon(null)} />}
      </div>
    </div>
  )
}
