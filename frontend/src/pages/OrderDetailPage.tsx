import { useEffect, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import { OrderItems, OrderStatusBadge, OrderTotals } from '@/components/order/OrderParts'
import { donHangApi } from '@/services/api/donHangApi'
import type { DonHangChiTiet, TrangThaiDonHang } from '@/types/donHang'
import { NHAN_GIAO_HANG, NHAN_THANH_TOAN, NHAN_TRANG_THAI_DON, dinhDangNgayGio } from '@/utils/format'
import { thongBaoLoi, useAsync } from '@/utils/useAsync'

const LUONG: TrangThaiDonHang[] = ['ChoXacNhan', 'DaXacNhan', 'DangDongGoi', 'DangGiao', 'HoanTat']

function TienTrinh({ trangThai }: { trangThai: TrangThaiDonHang }) {
  if (trangThai === 'DaHuy') {
    return <p className="rounded-xl bg-gray-100 px-4 py-3 text-sm text-gray-700">Đơn hàng đã bị hủy. Sản phẩm đã được hoàn về kho.</p>
  }
  const viTri = LUONG.indexOf(trangThai)
  return (
    <ol className="grid grid-cols-5 gap-1" aria-label="Tiến trình đơn hàng">
      {LUONG.map((tt, i) => (
        <li key={tt} className="text-center" aria-current={i === viTri ? 'step' : undefined}>
          <div className={`h-1.5 rounded-full ${i <= viTri ? 'bg-brand-500' : 'bg-gray-200'}`} />
          <p className={`mt-2 text-[11px] sm:text-xs ${i <= viTri ? 'font-medium text-brand-700' : 'text-gray-400'}`}>
            {NHAN_TRANG_THAI_DON[tt]}
          </p>
        </li>
      ))}
    </ol>
  )
}

export default function OrderDetailPage() {
  const { id } = useParams()
  const maDonHang = Number(id)
  const location = useLocation()
  const moiDat = (location.state as { moiDat?: boolean } | null)?.moiDat === true

  const { duLieu, dangTai, loi, taiLai } = useAsync(
    (signal) =>
      Number.isInteger(maDonHang) && maDonHang > 0
        ? donHangApi.chiTiet(maDonHang, signal)
        : Promise.reject(new Error('Mã đơn hàng không hợp lệ.')),
    [maDonHang],
  )
  const [don, setDon] = useState<DonHangChiTiet | null>(null)
  const [dangHuy, setDangHuy] = useState(false)
  const [loiHuy, setLoiHuy] = useState<string | null>(null)

  useEffect(() => setDon(duLieu), [duLieu])

  if (loi) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16">
        <HopLoi thongBao={loi} onThuLai={taiLai} />
        <p className="mt-4 text-center">
          <Link to="/don-hang" className="text-sm text-brand-600 hover:underline">
            ← Đơn hàng của tôi
          </Link>
        </p>
      </div>
    )
  }
  if (dangTai || !don) return <DangTai />

  async function huyDon() {
    if (!window.confirm(`Hủy đơn hàng #${don!.maDonHang}?`)) return
    setDangHuy(true)
    setLoiHuy(null)
    try {
      setDon(await donHangApi.huy(don!.maDonHang))
    } catch (err) {
      setLoiHuy(thongBaoLoi(err))
    } finally {
      setDangHuy(false)
    }
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <Link to="/don-hang" className="text-sm text-gray-500 hover:text-brand-600">
        ← Đơn hàng của tôi
      </Link>

      {moiDat && don.trangThai === 'ChoXacNhan' && (
        <div className="mt-4 rounded-2xl border border-green-100 bg-green-50 p-5 text-green-800" role="status">
          <p className="font-semibold">Đặt hàng thành công!</p>
          <p className="mt-1 text-sm">Cửa hàng sẽ xác nhận đơn sớm. Bạn có thể theo dõi trạng thái đơn tại đây.</p>
        </div>
      )}

      <div className="mt-4 flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-gray-900">Đơn #{don.maDonHang}</h1>
          <OrderStatusBadge trangThai={don.trangThai} />
        </div>
        {don.coTheHuy && (
          <button
            type="button"
            onClick={huyDon}
            disabled={dangHuy}
            className="rounded-full border border-red-200 px-4 py-1.5 text-sm font-medium text-red-600 hover:bg-red-50 disabled:opacity-60"
          >
            {dangHuy ? 'Đang hủy…' : 'Hủy đơn'}
          </button>
        )}
      </div>
      <p className="mt-1 text-sm text-gray-500">
        Đặt lúc {dinhDangNgayGio(don.ngayDat)}
        {don.ngayCapNhat && ` · cập nhật ${dinhDangNgayGio(don.ngayCapNhat)}`}
      </p>
      {loiHuy && (
        <p className="mt-3 rounded-lg bg-red-50 px-4 py-2 text-sm text-red-700" role="alert">
          {loiHuy}
        </p>
      )}

      <div className="mt-6">
        <TienTrinh trangThai={don.trangThai} />
      </div>

      <div className="mt-8 grid gap-8 md:grid-cols-[1fr_300px]">
        <section>
          <h2 className="font-semibold text-gray-900">Sản phẩm</h2>
          <OrderItems dong={don.sanPham} hienDanhGia={don.trangThai === 'HoanTat'} />
        </section>

        <aside className="space-y-6">
          <div className="rounded-2xl bg-gray-50 p-4">
            <OrderTotals tienHang={don.tienHang} tienGiam={don.tienGiam} phiVanChuyen={don.phiVanChuyen} maCode={don.maCode} />
            <p className="mt-3 text-xs text-gray-500">
              {NHAN_THANH_TOAN[don.phuongThucThanhToan]} ·{' '}
              <span className={don.daThanhToan ? 'text-green-700' : 'text-amber-700'}>
                {don.daThanhToan ? 'Đã thanh toán' : 'Chưa thanh toán'}
              </span>
            </p>
          </div>
          <div className="text-sm">
            <h2 className="font-semibold text-gray-900">Giao đến</h2>
            <p className="mt-2 text-gray-900">{don.tenNguoiNhan}</p>
            <p className="text-gray-600">{don.soDienThoai}</p>
            <p className="text-gray-600">{don.diaChiGiaoHang}</p>
            <p className="mt-2 text-xs text-gray-500">{NHAN_GIAO_HANG[don.phuongThucGiaoHang]}</p>
          </div>
        </aside>
      </div>
    </div>
  )
}
