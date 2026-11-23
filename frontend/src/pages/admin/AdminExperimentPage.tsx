import { useCallback, useEffect, useRef, useState } from 'react'
import Pagination from '@/components/common/Pagination'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import { adminAiApi } from '@/services/api/aiApi'
import type { ChiSoMoHinh, MoHinhAbsa, TrangThaiAi, TrangThaiThucNghiem } from '@/types/ai'
import { NHAN_CAM_XUC, dinhDangNgayGio } from '@/utils/format'
import { thongBaoLoi, useAsync } from '@/utils/useAsync'

const MO_HINH: { ma: MoHinhAbsa; nhan: string; moTa: string }[] = [
  { ma: 'PhoBERT', nhan: 'PhoBERT', moTa: 'wonrax/phobert-base-vietnamese-sentiment + từ điển khía cạnh' },
  { ma: 'LLM_KhongVD', nhan: 'LLM zero-shot', moTa: 'Claude, không ví dụ' },
  { ma: 'LLM_ItVD', nhan: 'LLM few-shot', moTa: 'Claude + ví dụ từ tập ViDuMau' },
]

const pt = (x: number) => `${(x * 100).toFixed(1)}%`

/** Ô số kèm thanh nhỏ một màu (0..1) – so sánh nhanh giữa các mô hình. */
function OThanh({ giaTri }: { giaTri: number }) {
  return (
    <div className="flex items-center justify-end gap-2">
      <span className="tabular-nums">{pt(giaTri)}</span>
      <span className="h-1.5 w-16 overflow-hidden rounded-full bg-gray-100" aria-hidden>
        <span className="block h-full rounded-full" style={{ width: `${giaTri * 100}%`, background: '#2a78d6' }} />
      </span>
    </div>
  )
}

function BangChiSo({ moHinh, soKiemThu }: { moHinh: ChiSoMoHinh[]; soKiemThu: number }) {
  if (moHinh.length === 0)
    return <p className="py-8 text-center text-sm text-gray-500">Chưa có kết quả thực nghiệm. Hãy chạy thực nghiệm bên dưới.</p>
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[760px] text-sm">
        <thead>
          <tr className="border-b border-gray-200 text-right text-xs text-gray-500">
            <th className="py-2 text-left font-medium">Cấu hình</th>
            <th className="py-2 font-medium">Số mẫu</th>
            <th className="py-2 font-medium">Accuracy</th>
            <th className="py-2 font-medium">Precision (macro)</th>
            <th className="py-2 font-medium">Recall (macro)</th>
            <th className="py-2 font-medium">F1 (macro)</th>
            <th className="py-2 font-medium">F1 khía cạnh</th>
            <th className="py-2 font-medium">Đúng cảm xúc KC</th>
            <th className="py-2 font-medium">Độ trễ TB</th>
          </tr>
        </thead>
        <tbody>
          {moHinh.map((m) => {
            const thieu = m.soMau < soKiemThu
            return (
              <tr key={m.moHinh} className="border-b border-gray-100 text-right last:border-0">
                <td className="py-3 text-left">
                  <p className="font-medium text-gray-900">{MO_HINH.find((x) => x.ma === m.moHinh)?.nhan ?? m.moHinh}</p>
                  <p className="text-xs text-gray-400">
                    {m.moHinh}
                    {m.ngayChayGanNhat && ` · ${dinhDangNgayGio(m.ngayChayGanNhat)}`}
                  </p>
                  {thieu && (
                    <p className="mt-1 text-xs text-amber-700">
                      ⚠ Chỉ {m.soMau}/{soKiemThu} mẫu – chưa chạy đủ bộ kiểm thử, không dùng để so sánh.
                    </p>
                  )}
                </td>
                <td className="py-3 tabular-nums">{m.soMau}</td>
                <td className="py-3"><OThanh giaTri={m.doChinhXac} /></td>
                <td className="py-3 tabular-nums">{pt(m.doChuanXacMacro)}</td>
                <td className="py-3 tabular-nums">{pt(m.doPhuMacro)}</td>
                <td className="py-3"><OThanh giaTri={m.diemF1Macro} /></td>
                <td className="py-3"><OThanh giaTri={m.khiaCanh.diemF1} /></td>
                <td className="py-3 tabular-nums">{pt(m.khiaCanh.doChinhXacCamXuc)}</td>
                <td className="py-3 tabular-nums">{m.doTreTrungBinhMs !== null ? `${Math.round(m.doTreTrungBinhMs)} ms` : '–'}</td>
              </tr>
            )
          })}
        </tbody>
      </table>
      <p className="mt-2 text-xs text-gray-400">
        Accuracy / Precision / Recall / F1 (macro) cho cảm xúc tổng lấy từ <code>vw_ChiSoThucNghiem</code>. F1 khía cạnh
        (micro, phát hiện khía cạnh) và “đúng cảm xúc KC” (trên khía cạnh phát hiện đúng) tính từ KhiaCanhChuan /
        KhiaCanhDuDoan.
      </p>
    </div>
  )
}

function DieuKhienChay({ onXong }: { onXong: () => void }) {
  const [ai, setAi] = useState<TrangThaiAi | null>(null)
  const [chon, setChon] = useState<MoHinhAbsa[]>([])
  const [tt, setTt] = useState<TrangThaiThucNghiem | null>(null)
  const [thongBao, setThongBao] = useState<string | null>(null)
  const hen = useRef<number | null>(null)

  const thamDo = useCallback(async () => {
    try {
      const s = await adminAiApi.trangThaiThucNghiem()
      setTt(s)
      if (s.dang_chay) hen.current = window.setTimeout(thamDo, 1500)
      else onXong()
    } catch (e) {
      setThongBao(thongBaoLoi(e))
    }
  }, [onXong])

  useEffect(() => {
    adminAiApi
      .trangThai(true)
      .then((s) => {
        setAi(s)
        setChon(MO_HINH.filter((m) => s.mo_hinh[m.ma]?.san_sang).map((m) => m.ma))
      })
      .catch((e) => setThongBao(thongBaoLoi(e)))
    adminAiApi.trangThaiThucNghiem().then((s) => {
      setTt(s)
      if (s.dang_chay) void thamDo()
    }).catch(() => {})
    return () => {
      if (hen.current) window.clearTimeout(hen.current)
    }
  }, [thamDo])

  async function chay() {
    setThongBao(null)
    try {
      await adminAiApi.chay(chon)
      void thamDo()
    } catch (e) {
      setThongBao(thongBaoLoi(e))
    }
  }

  async function napMau() {
    setThongBao(null)
    try {
      const kq = await adminAiApi.napMau()
      setThongBao(`Đã thêm ${kq.da_them} mẫu, bỏ qua ${kq.bo_qua_trung} mẫu đã có.`)
      onXong()
    } catch (e) {
      setThongBao(thongBaoLoi(e))
    }
  }

  return (
    <section className="rounded-2xl border border-gray-100 bg-white p-5">
      <h2 className="font-semibold text-gray-900">Chạy thực nghiệm</h2>
      <p className="mt-1 text-xs text-gray-500">
        Chạy trên toàn bộ mẫu <code>KiemThu</code>; kết quả ghi đè vào <code>KetQuaThucNghiem</code> theo (MaMau, MoHinh). Cấu
        hình không chạy được trên máy (thiếu mô hình/khóa API) bị bỏ qua và không ghi kết quả.
      </p>
      <div className="mt-4 grid gap-2 sm:grid-cols-3">
        {MO_HINH.map((m) => {
          const s = ai?.mo_hinh[m.ma]
          return (
            <label
              key={m.ma}
              className={`flex cursor-pointer gap-2 rounded-xl border p-3 text-sm ${
                chon.includes(m.ma) ? 'border-brand-500 bg-brand-50' : 'border-gray-200'
              } ${s && !s.san_sang ? 'opacity-60' : ''}`}
            >
              <input
                type="checkbox"
                className="mt-0.5 accent-brand-500"
                checked={chon.includes(m.ma)}
                disabled={!s?.san_sang}
                onChange={(e) => setChon((c) => (e.target.checked ? [...c, m.ma] : c.filter((x) => x !== m.ma)))}
              />
              <span>
                <span className="font-medium text-gray-900">{m.nhan}</span>
                <span className="block text-xs text-gray-500">{m.moTa}</span>
                <span className={`block text-xs ${s?.san_sang ? 'text-green-700' : 'text-gray-500'}`}>
                  {!ai ? 'Đang kiểm tra…' : s?.san_sang ? 'Sẵn sàng' : s?.ly_do ?? 'Không chạy được'}
                </span>
              </span>
            </label>
          )
        })}
      </div>

      {tt?.dang_chay && (
        <div className="mt-4 space-y-2 text-xs" role="status">
          {Object.entries(tt.tien_do).map(([ma, p]) => (
            <div key={ma}>
              <p className="text-gray-600">
                {ma}: {p!.da_xong}/{p!.tong}
              </p>
              <div className="mt-1 h-1.5 overflow-hidden rounded-full bg-gray-100">
                <div className="h-full bg-brand-500" style={{ width: `${(p!.da_xong / Math.max(1, p!.tong)) * 100}%` }} />
              </div>
            </div>
          ))}
        </div>
      )}
      {tt && !tt.dang_chay && tt.ket_thuc && (
        <div className="mt-4 text-xs text-gray-500">
          Lượt gần nhất kết thúc {dinhDangNgayGio(tt.ket_thuc.replace(' ', 'T'))}
          {tt.bo_qua && Object.keys(tt.bo_qua).length > 0 && (
            <ul className="mt-1 text-amber-700">
              {Object.entries(tt.bo_qua).map(([ma, lyDo]) => (
                <li key={ma}>
                  Bỏ qua {ma}: {lyDo}
                </li>
              ))}
            </ul>
          )}
          {Object.entries(tt.loi).map(([ma, l]) => (
            <p key={ma} className="text-red-600">
              {ma}: {l}
            </p>
          ))}
        </div>
      )}

      <div className="mt-4 flex flex-wrap gap-2">
        <button
          type="button"
          onClick={() => void chay()}
          disabled={chon.length === 0 || !!tt?.dang_chay}
          className="rounded-lg bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-50"
        >
          {tt?.dang_chay ? 'Đang chạy…' : `Chạy ${chon.length} cấu hình`}
        </button>
        <button
          type="button"
          onClick={() => void napMau()}
          className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
        >
          Nạp bộ mẫu kiểm thử
        </button>
      </div>
      {thongBao && <p className="mt-3 text-sm text-gray-700">{thongBao}</p>}
    </section>
  )
}

function KetQuaTungMau() {
  const [moHinh, setMoHinh] = useState<MoHinhAbsa>('PhoBERT')
  const [chiSai, setChiSai] = useState(true)
  const [trang, setTrang] = useState(1)
  const { duLieu, dangTai, loi } = useAsync(() => adminAiApi.ketQua(moHinh, chiSai, trang), [moHinh, chiSai, trang])

  const kc = (d: Record<string, string>) =>
    Object.entries(d).map(([k, v]) => `${k}: ${NHAN_CAM_XUC[v as keyof typeof NHAN_CAM_XUC] ?? v}`).join(' · ') || '—'

  return (
    <section className="rounded-2xl border border-gray-100 bg-white p-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="font-semibold text-gray-900">Kết quả từng mẫu</h2>
        <div className="flex flex-wrap items-center gap-3 text-sm">
          <select
            value={moHinh}
            onChange={(e) => {
              setMoHinh(e.target.value as MoHinhAbsa)
              setTrang(1)
            }}
            className="rounded-lg border border-gray-300 px-2 py-1.5"
          >
            {MO_HINH.map((m) => (
              <option key={m.ma} value={m.ma}>
                {m.nhan}
              </option>
            ))}
          </select>
          <label className="flex items-center gap-2 text-gray-600">
            <input
              type="checkbox"
              checked={chiSai}
              onChange={(e) => {
                setChiSai(e.target.checked)
                setTrang(1)
              }}
              className="accent-brand-500"
            />
            Chỉ mẫu sai
          </label>
        </div>
      </div>

      {loi ? (
        <p className="mt-4 text-sm text-red-600">{loi}</p>
      ) : dangTai && !duLieu ? (
        <DangTai />
      ) : duLieu && duLieu.duLieu.length === 0 ? (
        <p className="py-8 text-center text-sm text-gray-500">Không có mẫu nào.</p>
      ) : (
        duLieu && (
          <>
            <div className="mt-4 overflow-x-auto">
              <table className="w-full min-w-[760px] text-sm">
                <thead>
                  <tr className="border-b border-gray-200 text-left text-xs text-gray-500">
                    <th className="py-2 font-medium">#</th>
                    <th className="py-2 font-medium">Nội dung</th>
                    <th className="py-2 font-medium">Chuẩn → Dự đoán</th>
                    <th className="py-2 font-medium">Khía cạnh (chuẩn / dự đoán)</th>
                    <th className="py-2 font-medium">Nhóm lỗi</th>
                  </tr>
                </thead>
                <tbody>
                  {duLieu.duLieu.map((m) => (
                    <tr key={m.maMau} className="border-b border-gray-100 align-top last:border-0">
                      <td className="py-2 text-xs text-gray-400">{m.maMau}</td>
                      <td className="max-w-xs py-2 text-gray-800">{m.noiDung}</td>
                      <td className="py-2 text-xs">
                        <span className={m.laDung ? 'text-green-700' : 'text-red-600'}>
                          {m.laDung ? '✓' : '✕'} {NHAN_CAM_XUC[m.camXucChuan]} → {NHAN_CAM_XUC[m.camXucDuDoan]}
                        </span>
                      </td>
                      <td className="py-2 text-xs text-gray-600">
                        <p>{kc(m.khiaCanhChuan)}</p>
                        <p className="text-gray-400">{kc(m.khiaCanhDuDoan)}</p>
                      </td>
                      <td className="py-2 text-xs text-gray-600">{m.nhomLoi ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="mt-4">
              <Pagination trang={duLieu.trang} tongSoTrang={duLieu.tongSoTrang} onChange={setTrang} />
            </div>
          </>
        )
      )}
    </section>
  )
}

export default function AdminExperimentPage() {
  const { duLieu, dangTai, loi, taiLai } = useAsync(() => adminAiApi.chiSo(), [])
  const [lan, setLan] = useState(0)
  const taiLaiTatCa = useCallback(() => {
    taiLai()
    setLan((x) => x + 1)
  }, [taiLai])

  return (
    <div className="mx-auto max-w-6xl space-y-6 px-4 py-8">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Thực nghiệm so sánh mô hình ABSA</h1>
        {duLieu && (
          <p className="mt-1 text-sm text-gray-500">
            Bộ mẫu: {duLieu.boMau.soKiemThu} mẫu kiểm thử (
            {Object.entries(duLieu.boMau.phanBoNhan)
              .map(([k, v]) => `${NHAN_CAM_XUC[k as keyof typeof NHAN_CAM_XUC]} ${v}`)
              .join(' · ')}
            ) và {duLieu.boMau.soViDuMau} ví dụ few-shot.
          </p>
        )}
      </div>

      <section className="rounded-2xl border border-gray-100 bg-white p-5">
        <h2 className="mb-3 font-semibold text-gray-900">Chỉ số theo cấu hình</h2>
        {loi ? (
          <HopLoi thongBao={loi} onThuLai={taiLai} />
        ) : dangTai && !duLieu ? (
          <DangTai />
        ) : (
          duLieu && <BangChiSo moHinh={duLieu.moHinh} soKiemThu={duLieu.boMau.soKiemThu} />
        )}
      </section>

      <DieuKhienChay onXong={taiLaiTatCa} />
      <KetQuaTungMau key={lan} />
    </div>
  )
}
