import { useEffect, useState } from 'react'
import { BangCamXuc, ChuGiaiCamXuc, CotXuHuong, ThanhKhiaCanh, type DongCamXuc } from '@/components/charts/SentimentCharts'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import { adminAiApi } from '@/services/api/aiApi'
import { sanPhamApi } from '@/services/api/sanPhamApi'
import type { TrangThaiAi } from '@/types/ai'
import type { SanPhamTomTat } from '@/types/sanPham'
import { thongBaoLoi, useAsync } from '@/utils/useAsync'

const KHOANG_NGAY = [7, 30, 90, 365]
const ngayNgan = new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit' })

function TheChiSo({ nhan, giaTri, phu }: { nhan: string; giaTri: string; phu?: string }) {
  return (
    <div className="rounded-2xl border border-gray-100 bg-white p-4">
      <p className="text-xs text-gray-500">{nhan}</p>
      <p className="mt-1 text-2xl font-semibold tabular-nums text-gray-900">{giaTri}</p>
      {phu && <p className="mt-0.5 text-xs text-gray-400">{phu}</p>}
    </div>
  )
}

function TrangThaiMoHinh() {
  const [tt, setTt] = useState<TrangThaiAi | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [thongBao, setThongBao] = useState<string | null>(null)
  const [dangDongBo, setDangDongBo] = useState(false)

  useEffect(() => {
    adminAiApi.trangThai(true).then(setTt).catch((e) => setLoi(thongBaoLoi(e)))
  }, [])

  async function dongBo(tatCa: boolean) {
    setDangDongBo(true)
    setThongBao(null)
    try {
      const kq = await adminAiApi.dongBo(tatCa)
      setThongBao(`Đã gửi ${kq.so_danh_gia} đánh giá sang ai-service để phân tích (xử lý nền).`)
    } catch (e) {
      setThongBao(thongBaoLoi(e))
    } finally {
      setDangDongBo(false)
    }
  }

  return (
    <div className="rounded-2xl border border-gray-100 bg-white p-4 text-sm">
      <p className="font-semibold text-gray-900">Dịch vụ AI</p>
      {loi ? (
        <p className="mt-2 text-red-600">{loi}</p>
      ) : !tt ? (
        <p className="mt-2 text-gray-500">Đang kiểm tra mô hình…</p>
      ) : (
        <ul className="mt-2 space-y-1 text-xs">
          {Object.entries(tt.mo_hinh).map(([ten, m]) => (
            <li key={ten} className="flex items-start gap-2">
              <span aria-hidden>{m.san_sang ? '✓' : '✕'}</span>
              <span>
                <span className="font-medium text-gray-800">{ten}</span>{' '}
                <span className={m.san_sang ? 'text-green-700' : 'text-gray-500'}>
                  {m.san_sang ? 'sẵn sàng' : m.ly_do ?? 'chưa khởi tạo'}
                </span>
              </span>
            </li>
          ))}
          <li className="pt-1 text-gray-500">
            Embedding: <span className="font-medium text-gray-800">{tt.embedding}</span>
            {tt.embedding_du_phong && <span className="text-amber-700"> (dự phòng: {tt.embedding_du_phong})</span>}
          </li>
        </ul>
      )}
      <div className="mt-3 flex flex-wrap gap-2">
        <button
          type="button"
          disabled={dangDongBo}
          onClick={() => void dongBo(false)}
          className="rounded-lg bg-brand-500 px-3 py-1.5 text-xs font-medium text-white hover:bg-brand-600 disabled:opacity-60"
        >
          Phân tích đánh giá còn thiếu
        </button>
        <button
          type="button"
          disabled={dangDongBo}
          onClick={() => window.confirm('Phân tích lại TOÀN BỘ đánh giá đã duyệt?') && void dongBo(true)}
          className="rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-60"
        >
          Phân tích lại tất cả
        </button>
      </div>
      {thongBao && <p className="mt-2 text-xs text-gray-600">{thongBao}</p>}
    </div>
  )
}

export default function AdminDashboardPage() {
  const [maSanPham, setMaSanPham] = useState<number | undefined>(undefined)
  const [soNgay, setSoNgay] = useState(30)
  const [xemBang, setXemBang] = useState(false)
  const [sanPham, setSanPham] = useState<SanPhamTomTat[]>([])

  useEffect(() => {
    sanPhamApi.timKiem({ kichThuoc: 50 }).then((kq) => setSanPham(kq.duLieu)).catch(() => setSanPham([]))
  }, [])

  const { duLieu, dangTai, loi, taiLai } = useAsync(() => adminAiApi.bangDieuKhien(maSanPham, soNgay), [maSanPham, soNgay])

  const dongKhiaCanh: DongCamXuc[] = (duLieu?.khiaCanh ?? []).map((k) => ({
    khoa: String(k.maKhiaCanh),
    nhan: k.tenKhiaCanh,
    dem: { TichCuc: k.soTichCuc, TrungTinh: k.soTrungTinh, TieuCuc: k.soTieuCuc },
  }))
  const dongXuHuong: DongCamXuc[] = (duLieu?.xuHuong ?? []).map((x) => ({
    khoa: x.ngay,
    nhan: ngayNgan.format(new Date(`${x.ngay}T00:00:00`)),
    dem: { TichCuc: x.soTichCuc, TrungTinh: x.soTrungTinh, TieuCuc: x.soTieuCuc },
  }))

  const tongDanhGia = dongXuHuong.reduce((s, d) => s + d.dem.TichCuc + d.dem.TrungTinh + d.dem.TieuCuc, 0)
  const tongTichCuc = dongXuHuong.reduce((s, d) => s + d.dem.TichCuc, 0)
  const tongTieuCuc = dongXuHuong.reduce((s, d) => s + d.dem.TieuCuc, 0)
  const tongY = dongKhiaCanh.reduce((s, d) => s + d.dem.TichCuc + d.dem.TrungTinh + d.dem.TieuCuc, 0)

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">Thống kê cảm xúc & khía cạnh</h1>
      <p className="mt-1 text-sm text-gray-500">
        Nguồn: <code>vw_XuHuongCamXuc</code> và <code>vw_ThongKeKhiaCanh</code> – chỉ đánh giá đã duyệt và đã được AI phân tích.
      </p>

      {/* Bộ lọc – một hàng phía trên biểu đồ */}
      <div className="mt-6 flex flex-wrap items-end gap-4">
        <label className="text-sm">
          <span className="block text-xs text-gray-500">Sản phẩm</span>
          <select
            value={maSanPham ?? ''}
            onChange={(e) => setMaSanPham(e.target.value ? Number(e.target.value) : undefined)}
            className="mt-1 rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-brand-500 focus:outline-none"
          >
            <option value="">Tất cả sản phẩm</option>
            {sanPham.map((s) => (
              <option key={s.maSanPham} value={s.maSanPham}>
                {s.ten}
              </option>
            ))}
          </select>
        </label>
        <div className="text-sm">
          <span className="block text-xs text-gray-500">Khoảng thời gian</span>
          <div className="mt-1 inline-flex overflow-hidden rounded-lg border border-gray-300" role="group">
            {KHOANG_NGAY.map((n) => (
              <button
                key={n}
                type="button"
                aria-pressed={soNgay === n}
                onClick={() => setSoNgay(n)}
                className={`px-3 py-1.5 text-sm ${soNgay === n ? 'bg-brand-500 text-white' : 'text-gray-600 hover:bg-gray-50'}`}
              >
                {n} ngày
              </button>
            ))}
          </div>
        </div>
        <label className="ml-auto flex items-center gap-2 text-sm text-gray-600">
          <input type="checkbox" checked={xemBang} onChange={(e) => setXemBang(e.target.checked)} className="accent-brand-500" />
          Xem dạng bảng
        </label>
      </div>

      {loi ? (
        <div className="mt-6">
          <HopLoi thongBao={loi} onThuLai={taiLai} />
        </div>
      ) : dangTai && !duLieu ? (
        <DangTai />
      ) : (
        <div className={`mt-6 grid gap-6 lg:grid-cols-[1fr_300px] ${dangTai ? 'opacity-60' : ''}`}>
          <div className="space-y-6">
            <div className="grid gap-4 sm:grid-cols-3">
              <TheChiSo nhan={`Đánh giá đã phân tích (${soNgay} ngày)`} giaTri={String(tongDanhGia)} />
              <TheChiSo
                nhan="Tỉ lệ tích cực"
                giaTri={tongDanhGia ? `${Math.round((tongTichCuc / tongDanhGia) * 100)}%` : '–'}
                phu={`${tongTichCuc} đánh giá`}
              />
              <TheChiSo
                nhan="Tỉ lệ tiêu cực"
                giaTri={tongDanhGia ? `${Math.round((tongTieuCuc / tongDanhGia) * 100)}%` : '–'}
                phu={`${tongTieuCuc} đánh giá`}
              />
            </div>

            <section className="rounded-2xl border border-gray-100 bg-white p-5">
              <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
                <h2 className="font-semibold text-gray-900">Xu hướng cảm xúc theo ngày</h2>
                <ChuGiaiCamXuc />
              </div>
              {xemBang ? <BangCamXuc dong={dongXuHuong} cotDau="Ngày" /> : <CotXuHuong dong={dongXuHuong} />}
            </section>

            <section className="rounded-2xl border border-gray-100 bg-white p-5">
              <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
                <h2 className="font-semibold text-gray-900">Cảm xúc theo khía cạnh ({tongY} ý kiến)</h2>
                <ChuGiaiCamXuc />
              </div>
              {xemBang ? <BangCamXuc dong={dongKhiaCanh} cotDau="Khía cạnh" /> : <ThanhKhiaCanh dong={dongKhiaCanh} />}
              <p className="mt-3 text-xs text-gray-400">Tính trên toàn bộ đánh giá đã duyệt (không giới hạn khoảng thời gian).</p>
            </section>
          </div>

          <aside>
            <TrangThaiMoHinh />
          </aside>
        </div>
      )}
    </div>
  )
}
