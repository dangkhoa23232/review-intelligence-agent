import { useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import Pagination from '@/components/common/Pagination'
import { StarRating } from '@/components/common/StarRating'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import { adminDanhGiaApi } from '@/services/api/danhGiaApi'
import type { DanhGiaQuanTri, TrangThaiDanhGia } from '@/types/danhGia'
import { MAU_CAM_XUC, NHAN_CAM_XUC, NHAN_TRANG_THAI_DANH_GIA, dinhDangNgayGio } from '@/utils/format'
import { thongBaoLoi, useAsync } from '@/utils/useAsync'

const CAC_TAB: TrangThaiDanhGia[] = ['ChoDuyet', 'DaDuyet', 'DaAn']
const KICH_THUOC_TRANG = 10

type HanhDong = Extract<TrangThaiDanhGia, 'DaDuyet' | 'DaAn'>

function TheDanhGia({
  dg,
  dangXuLy,
  onHanhDong,
}: {
  dg: DanhGiaQuanTri
  dangXuLy: boolean
  onHanhDong: (dg: DanhGiaQuanTri, tt: HanhDong) => void
}) {
  return (
    <article className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
      <header className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <Link to={`/san-pham/${dg.duongDanSanPham}`} className="font-semibold text-gray-900 hover:text-brand-600">
            {dg.tenSanPham}
          </Link>
          <p className="mt-0.5 text-xs text-gray-500">
            #{dg.maDanhGia} · {dg.tenNguoiDung} ({dg.email}) · {dinhDangNgayGio(dg.ngayTao)}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          {dg.daMuaHang && (
            <span className="rounded-full bg-green-50 px-2 py-0.5 text-xs font-medium text-green-700 ring-1 ring-green-600/20 ring-inset">
              ✓ Đã mua hàng
            </span>
          )}
          {dg.camXuc && (
            <span className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${MAU_CAM_XUC[dg.camXuc]}`}>
              AI: {NHAN_CAM_XUC[dg.camXuc]}
            </span>
          )}
          {dg.soBaoCaoChoXuLy > 0 && (
            <span className="rounded-full bg-amber-50 px-2 py-0.5 text-xs font-medium text-amber-700 ring-1 ring-amber-600/20 ring-inset">
              {dg.soBaoCaoChoXuLy} báo cáo chờ xử lý
            </span>
          )}
        </div>
      </header>

      <div className="mt-3">
        <StarRating diem={dg.soSao} kichThuoc={15} />
      </div>
      <p className="mt-2 whitespace-pre-line text-sm text-gray-700">{dg.noiDung}</p>

      {dg.diemTieuChi.length > 0 && (
        <ul className="mt-3 flex flex-wrap gap-2">
          {dg.diemTieuChi.map((c) => (
            <li key={c.maKhiaCanh} className="rounded-md bg-gray-50 px-2 py-1 text-xs text-gray-600">
              {c.tenKhiaCanh}: <span className="font-semibold text-gray-900">{c.diem}/5</span>
            </li>
          ))}
        </ul>
      )}

      <footer className="mt-4 flex flex-wrap items-center gap-2 border-t border-gray-100 pt-4">
        {dg.trangThai !== 'DaDuyet' && (
          <button
            type="button"
            disabled={dangXuLy}
            onClick={() => onHanhDong(dg, 'DaDuyet')}
            className="rounded-lg bg-green-600 px-4 py-1.5 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-60"
          >
            Duyệt
          </button>
        )}
        {dg.trangThai !== 'DaAn' && (
          <button
            type="button"
            disabled={dangXuLy}
            onClick={() => onHanhDong(dg, 'DaAn')}
            className="rounded-lg border border-gray-300 px-4 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-60"
          >
            Ẩn
          </button>
        )}
        {dg.ngayCapNhat && (
          <span className="ml-auto text-xs text-gray-400">Cập nhật {dinhDangNgayGio(dg.ngayCapNhat)}</span>
        )}
      </footer>
    </article>
  )
}

export default function AdminReviewsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const tabUrl = searchParams.get('trangThai') as TrangThaiDanhGia | null
  const tab: TrangThaiDanhGia = tabUrl && CAC_TAB.includes(tabUrl) ? tabUrl : 'ChoDuyet'
  const trang = Math.max(1, Number(searchParams.get('trang')) || 1)

  const [dangXuLy, setDangXuLy] = useState<number | null>(null)
  const [thongBao, setThongBao] = useState<{ loai: 'ok' | 'loi'; noiDung: string } | null>(null)

  const { duLieu, dangTai, loi, taiLai } = useAsync(
    () => adminDanhGiaApi.danhSach({ trangThai: tab, trang, kichThuoc: KICH_THUOC_TRANG }),
    [tab, trang],
  )

  const chuyenTab = (t: TrangThaiDanhGia) => setSearchParams({ trangThai: t })
  const chuyenTrang = (p: number) => setSearchParams({ trangThai: tab, trang: String(p) })

  async function xuLy(dg: DanhGiaQuanTri, trangThai: HanhDong) {
    setDangXuLy(dg.maDanhGia)
    setThongBao(null)
    try {
      const kq = await adminDanhGiaApi.doiTrangThai(dg.maDanhGia, trangThai)
      const hanhDong = trangThai === 'DaDuyet' ? 'Đã duyệt' : 'Đã ẩn'
      const ai = kq.daGuiPhanTichAi
        ? ' Đã gửi sang AI phân tích cảm xúc, khía cạnh và lưu vector.'
        : kq.daGuiGoBoVector
          ? ' Đã gửi AI gỡ vector khỏi Qdrant và cập nhật tóm tắt.'
          : ''
      setThongBao({
        loai: 'ok',
        noiDung: `${hanhDong} đánh giá #${kq.maDanhGia}. “${dg.tenSanPham}” hiện có ${kq.sanPham.diemTrungBinh.toFixed(2)}★ từ ${kq.sanPham.soDanhGia} đánh giá.${ai}`,
      })
      // Trang hiện tại trống sau khi chuyển đánh giá cuối cùng → lùi một trang.
      if (duLieu && duLieu.ketQua.duLieu.length === 1 && trang > 1) chuyenTrang(trang - 1)
      else taiLai()
    } catch (err) {
      setThongBao({ loai: 'loi', noiDung: thongBaoLoi(err) })
    } finally {
      setDangXuLy(null)
    }
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">Duyệt đánh giá</h1>
      <p className="mt-1 text-sm text-gray-500">
        Đánh giá chỉ hiển thị công khai sau khi được duyệt. Điểm trung bình sản phẩm được tính lại tự động.
      </p>

      <div className="mt-6 flex gap-1 border-b border-gray-200" role="tablist">
        {CAC_TAB.map((t) => (
          <button
            key={t}
            type="button"
            role="tab"
            aria-selected={t === tab}
            onClick={() => chuyenTab(t)}
            className={`-mb-px border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
              t === tab ? 'border-brand-500 text-brand-600' : 'border-transparent text-gray-500 hover:text-gray-800'
            }`}
          >
            {NHAN_TRANG_THAI_DANH_GIA[t]}
            {duLieu && (
              <span className="ml-2 rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">
                {duLieu.soLuongTheoTrangThai[t] ?? 0}
              </span>
            )}
          </button>
        ))}
      </div>

      {thongBao && (
        <p
          role="status"
          className={`mt-4 rounded-lg px-4 py-3 text-sm ${
            thongBao.loai === 'ok' ? 'bg-green-50 text-green-800' : 'bg-red-50 text-red-700'
          }`}
        >
          {thongBao.noiDung}
        </p>
      )}

      <div className="mt-6">
        {loi ? (
          <HopLoi thongBao={loi} onThuLai={taiLai} />
        ) : dangTai && !duLieu ? (
          <DangTai />
        ) : duLieu && duLieu.ketQua.duLieu.length === 0 ? (
          <p className="rounded-2xl bg-gray-50 px-4 py-16 text-center text-sm text-gray-500">
            Không có đánh giá nào ở trạng thái “{NHAN_TRANG_THAI_DANH_GIA[tab]}”.
          </p>
        ) : (
          duLieu && (
            <div className={`space-y-4 ${dangTai ? 'opacity-60' : ''}`}>
              {duLieu.ketQua.duLieu.map((dg) => (
                <TheDanhGia key={dg.maDanhGia} dg={dg} dangXuLy={dangXuLy === dg.maDanhGia} onHanhDong={xuLy} />
              ))}
              <div className="pt-4">
                <Pagination trang={duLieu.ketQua.trang} tongSoTrang={duLieu.ketQua.tongSoTrang} onChange={chuyenTrang} />
              </div>
            </div>
          )
        )}
      </div>
    </div>
  )
}
