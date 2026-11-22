import { useState } from 'react'
import { useSearchParams } from 'react-router'
import Pagination from '@/components/common/Pagination'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import { OrderItems, OrderStatusBadge, OrderTotals } from '@/components/order/OrderParts'
import { adminDonHangApi } from '@/services/api/donHangApi'
import type { DonHangChiTiet, TrangThaiDonHang } from '@/types/donHang'
import {
  NHAN_GIAO_HANG,
  NHAN_HANH_DONG_DON,
  NHAN_THANH_TOAN,
  NHAN_TRANG_THAI_DON,
  dinhDangNgayGio,
} from '@/utils/format'
import { thongBaoLoi, useAsync } from '@/utils/useAsync'

const CAC_TAB: TrangThaiDonHang[] = ['ChoXacNhan', 'DaXacNhan', 'DangDongGoi', 'DangGiao', 'HoanTat', 'DaHuy']
const KICH_THUOC_TRANG = 10

function TheDonHang({
  don,
  dangXuLy,
  onChuyen,
}: {
  don: DonHangChiTiet
  dangXuLy: boolean
  onChuyen: (don: DonHangChiTiet, tt: TrangThaiDonHang) => void
}) {
  return (
    <article className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
      <header className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <div className="flex items-center gap-2">
            <h2 className="font-semibold text-gray-900">Đơn #{don.maDonHang}</h2>
            <OrderStatusBadge trangThai={don.trangThai} />
          </div>
          <p className="mt-0.5 text-xs text-gray-500">
            {don.tenKhachHang} ({don.emailKhachHang}) · đặt {dinhDangNgayGio(don.ngayDat)}
          </p>
        </div>
        <div className="text-right text-xs text-gray-500">
          <p>{NHAN_GIAO_HANG[don.phuongThucGiaoHang]}</p>
          <p>
            {NHAN_THANH_TOAN[don.phuongThucThanhToan]} ·{' '}
            <span className={don.daThanhToan ? 'text-green-700' : 'text-amber-700'}>
              {don.daThanhToan ? 'đã thanh toán' : 'chưa thanh toán'}
            </span>
          </p>
        </div>
      </header>

      <div className="mt-4 grid gap-6 md:grid-cols-[1fr_260px]">
        <OrderItems dong={don.sanPham} />
        <div className="space-y-4">
          <div className="rounded-xl bg-gray-50 p-3 text-sm">
            <p className="font-medium text-gray-900">{don.tenNguoiNhan}</p>
            <p className="text-gray-600">{don.soDienThoai}</p>
            <p className="text-gray-600">{don.diaChiGiaoHang}</p>
          </div>
          <OrderTotals tienHang={don.tienHang} tienGiam={don.tienGiam} phiVanChuyen={don.phiVanChuyen} maCode={don.maCode} />
        </div>
      </div>

      {don.trangThaiTiepTheo.length > 0 && (
        <footer className="mt-4 flex flex-wrap items-center gap-2 border-t border-gray-100 pt-4">
          {don.trangThaiTiepTheo.map((tt) =>
            tt === 'DaHuy' ? (
              <button
                key={tt}
                type="button"
                disabled={dangXuLy}
                onClick={() => onChuyen(don, tt)}
                className="rounded-lg border border-red-200 px-4 py-1.5 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-60"
              >
                {NHAN_HANH_DONG_DON[tt]}
              </button>
            ) : (
              <button
                key={tt}
                type="button"
                disabled={dangXuLy}
                onClick={() => onChuyen(don, tt)}
                className="rounded-lg bg-brand-500 px-4 py-1.5 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-60"
              >
                {NHAN_HANH_DONG_DON[tt]} →
              </button>
            ),
          )}
          {don.ngayCapNhat && <span className="ml-auto text-xs text-gray-400">Cập nhật {dinhDangNgayGio(don.ngayCapNhat)}</span>}
        </footer>
      )}
    </article>
  )
}

export default function AdminOrdersPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const tabUrl = searchParams.get('trangThai') as TrangThaiDonHang | null
  const tab: TrangThaiDonHang = tabUrl && CAC_TAB.includes(tabUrl) ? tabUrl : 'ChoXacNhan'
  const trang = Math.max(1, Number(searchParams.get('trang')) || 1)

  const [dangXuLy, setDangXuLy] = useState<number | null>(null)
  const [thongBao, setThongBao] = useState<{ loai: 'ok' | 'loi'; noiDung: string } | null>(null)

  const { duLieu, dangTai, loi, taiLai } = useAsync(
    () => adminDonHangApi.danhSach({ trangThai: tab, trang, kichThuoc: KICH_THUOC_TRANG }),
    [tab, trang],
  )

  const chuyenTrang = (p: number) => setSearchParams({ trangThai: tab, trang: String(p) })

  async function chuyen(don: DonHangChiTiet, trangThai: TrangThaiDonHang) {
    if (trangThai === 'DaHuy' && !window.confirm(`Hủy đơn #${don.maDonHang}? Hàng sẽ được hoàn về kho.`)) return
    setDangXuLy(don.maDonHang)
    setThongBao(null)
    try {
      const kq = await adminDonHangApi.doiTrangThai(don.maDonHang, trangThai)
      setThongBao({ loai: 'ok', noiDung: `Đơn #${kq.maDonHang} → ${NHAN_TRANG_THAI_DON[kq.trangThai]}. Khách hàng đã nhận thông báo.` })
      // Đơn rời khỏi tab hiện tại; nếu là đơn cuối của trang thì lùi một trang.
      if (duLieu && duLieu.ketQua.duLieu.length === 1 && trang > 1) chuyenTrang(trang - 1)
      else taiLai()
    } catch (err) {
      setThongBao({ loai: 'loi', noiDung: thongBaoLoi(err) })
      taiLai()
    } finally {
      setDangXuLy(null)
    }
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">Quản lý đơn hàng</h1>
      <p className="mt-1 text-sm text-gray-500">
        Luồng xử lý: Chờ xác nhận → Đã xác nhận → Đang đóng gói → Đang giao → Hoàn tất. Hủy được trước khi giao (tự hoàn kho).
      </p>

      <div className="mt-6 flex gap-1 overflow-x-auto border-b border-gray-200" role="tablist">
        {CAC_TAB.map((t) => (
          <button
            key={t}
            type="button"
            role="tab"
            aria-selected={t === tab}
            onClick={() => setSearchParams({ trangThai: t })}
            className={`-mb-px shrink-0 border-b-2 px-3 py-2 text-sm font-medium transition-colors ${
              t === tab ? 'border-brand-500 text-brand-600' : 'border-transparent text-gray-500 hover:text-gray-800'
            }`}
          >
            {NHAN_TRANG_THAI_DON[t]}
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
            Không có đơn hàng ở trạng thái “{NHAN_TRANG_THAI_DON[tab]}”.
          </p>
        ) : (
          duLieu && (
            <div className={`space-y-4 ${dangTai ? 'opacity-60' : ''}`}>
              {duLieu.ketQua.duLieu.map((d) => (
                <TheDonHang key={d.maDonHang} don={d} dangXuLy={dangXuLy === d.maDonHang} onChuyen={chuyen} />
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
