import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router'
import QuantityStepper from '@/components/common/QuantityStepper'
import { DangTai } from '@/components/common/TrangThaiTai'
import ProductImage from '@/components/product/ProductImage'
import { useCart } from '@/context/useCart'
import { gioHangApi } from '@/services/api/gioHangApi'
import type { DongGioHang } from '@/types/gioHang'
import { dinhDangTien } from '@/utils/format'
import { thongBaoLoi } from '@/utils/useAsync'

function DongGio({
  d,
  dangXuLy,
  onSoLuong,
  onXoa,
}: {
  d: DongGioHang
  dangXuLy: boolean
  onSoLuong: (v: number) => void
  onXoa: () => void
}) {
  return (
    <li className={`flex gap-4 py-5 ${dangXuLy ? 'opacity-60' : ''}`}>
      <Link to={`/san-pham/${d.duongDan}`} className="shrink-0">
        <ProductImage src={d.anhDaiDien} alt={d.tenSanPham} className="h-24 w-24 rounded-xl text-2xl" />
      </Link>
      <div className="flex min-w-0 flex-1 flex-col gap-1">
        <Link to={`/san-pham/${d.duongDan}`} className="font-medium text-gray-900 hover:text-brand-600">
          {d.tenSanPham}
        </Link>
        <p className="text-xs text-gray-500">
          {d.dungTich} · SKU {d.maSKU}
        </p>
        <p className="text-sm">
          <span className="font-semibold text-gray-900">{dinhDangTien(d.donGia)}</span>
          {d.dangKhuyenMai && <span className="ml-2 text-xs text-gray-400 line-through">{dinhDangTien(d.giaBan)}</span>}
        </p>

        {!d.coTheMua ? (
          <p className="text-xs font-medium text-red-600">Sản phẩm đã ngừng bán hoặc hết hàng – vui lòng xóa khỏi giỏ.</p>
        ) : d.vuotTonKho ? (
          <p className="text-xs font-medium text-amber-600">Chỉ còn {d.soLuongTon} sản phẩm – vui lòng giảm số lượng.</p>
        ) : d.soLuongTon <= 10 ? (
          <p className="text-xs text-amber-600">Chỉ còn {d.soLuongTon} sản phẩm</p>
        ) : null}

        <div className="mt-auto flex flex-wrap items-center gap-4 pt-2">
          <QuantityStepper
            giaTri={d.soLuong}
            toiDa={Math.max(d.soLuongTon, 1)}
            onChange={onSoLuong}
            disabled={dangXuLy || !d.coTheMua}
            nhan={`Số lượng ${d.tenSanPham}`}
          />
          <button type="button" onClick={onXoa} disabled={dangXuLy} className="text-sm text-gray-500 hover:text-red-600">
            Xóa
          </button>
        </div>
      </div>
      <p className="text-right font-semibold text-gray-900">{dinhDangTien(d.thanhTien)}</p>
    </li>
  )
}

export default function CartPage() {
  const { gioHang, datGioHang, taiLai } = useCart()
  const navigate = useNavigate()
  const [dangTai, setDangTai] = useState(true)
  const [dangXuLy, setDangXuLy] = useState<number | 'tat-ca' | null>(null)
  const [loi, setLoi] = useState<string | null>(null)

  useEffect(() => {
    taiLai().finally(() => setDangTai(false))
  }, [taiLai])

  async function chay(ma: number | 'tat-ca', fn: () => Promise<Parameters<typeof datGioHang>[0]>) {
    setDangXuLy(ma)
    setLoi(null)
    try {
      datGioHang(await fn())
    } catch (err) {
      setLoi(thongBaoLoi(err))
      void taiLai()
    } finally {
      setDangXuLy(null)
    }
  }

  if (dangTai && !gioHang) return <DangTai />

  if (!gioHang || gioHang.danhSach.length === 0) {
    return (
      <div className="mx-auto max-w-xl px-4 py-24 text-center">
        <h1 className="text-2xl font-bold text-gray-900">Giỏ hàng trống</h1>
        <p className="mt-2 text-gray-600">Hãy khám phá các sản phẩm phù hợp với làn da của bạn.</p>
        <Link
          to="/san-pham"
          className="mt-6 inline-block rounded-full bg-brand-500 px-6 py-2.5 font-medium text-white hover:bg-brand-600"
        >
          Mua sắm ngay
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <div className="flex items-baseline justify-between gap-4">
        <h1 className="text-2xl font-bold text-gray-900">Giỏ hàng ({gioHang.tongSoLuong})</h1>
        <button
          type="button"
          disabled={dangXuLy !== null}
          onClick={() => window.confirm('Xóa toàn bộ sản phẩm trong giỏ hàng?') && chay('tat-ca', gioHangApi.xoaTatCa)}
          className="text-sm text-gray-500 hover:text-red-600"
        >
          Xóa tất cả
        </button>
      </div>

      {loi && (
        <p className="mt-4 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700" role="alert">
          {loi}
        </p>
      )}

      <div className="mt-6 grid gap-8 lg:grid-cols-[1fr_340px]">
        <ul className="divide-y divide-gray-100 border-y border-gray-100">
          {gioHang.danhSach.map((d) => (
            <DongGio
              key={d.maBienThe}
              d={d}
              dangXuLy={dangXuLy === d.maBienThe || dangXuLy === 'tat-ca'}
              onSoLuong={(v) => chay(d.maBienThe, () => gioHangApi.capNhat(d.maBienThe, v))}
              onXoa={() => chay(d.maBienThe, () => gioHangApi.xoaDong(d.maBienThe))}
            />
          ))}
        </ul>

        <aside className="h-fit rounded-2xl bg-gray-50 p-5 lg:sticky lg:top-24">
          <dl className="space-y-2 text-sm">
            <div className="flex justify-between">
              <dt className="text-gray-600">Tạm tính</dt>
              <dd className="text-lg font-semibold text-gray-900">{dinhDangTien(gioHang.tamTinh)}</dd>
            </div>
            <p className="text-xs text-gray-500">
              Phí vận chuyển từ {dinhDangTien(Math.min(...Object.values(gioHang.phiVanChuyen)))} và mã giảm giá được tính ở
              bước thanh toán.
            </p>
          </dl>
          {!gioHang.coTheDatHang && (
            <p className="mt-4 text-xs font-medium text-amber-700">Vui lòng xử lý các sản phẩm được đánh dấu trước khi đặt hàng.</p>
          )}
          <button
            type="button"
            disabled={!gioHang.coTheDatHang || dangXuLy !== null}
            onClick={() => navigate('/thanh-toan')}
            className="mt-5 w-full rounded-full bg-brand-500 py-3 font-medium text-white hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Tiến hành đặt hàng
          </button>
          <Link to="/san-pham" className="mt-3 block text-center text-sm text-brand-600 hover:underline">
            Tiếp tục mua sắm
          </Link>
        </aside>
      </div>
    </div>
  )
}
