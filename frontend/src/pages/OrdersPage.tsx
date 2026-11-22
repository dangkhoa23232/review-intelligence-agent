import { Link, useSearchParams } from 'react-router'
import Pagination from '@/components/common/Pagination'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import { OrderStatusBadge } from '@/components/order/OrderParts'
import ProductImage from '@/components/product/ProductImage'
import { donHangApi } from '@/services/api/donHangApi'
import { dinhDangNgayGio, dinhDangTien } from '@/utils/format'
import { useAsync } from '@/utils/useAsync'

export default function OrdersPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const trang = Math.max(1, Number(searchParams.get('trang')) || 1)
  const { duLieu, dangTai, loi, taiLai } = useAsync(() => donHangApi.danhSach(trang), [trang])

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">Đơn hàng của tôi</h1>

      <div className="mt-6">
        {loi ? (
          <HopLoi thongBao={loi} onThuLai={taiLai} />
        ) : dangTai && !duLieu ? (
          <DangTai />
        ) : duLieu && duLieu.duLieu.length === 0 ? (
          <div className="rounded-2xl bg-gray-50 px-4 py-16 text-center text-gray-500">
            Bạn chưa có đơn hàng nào.{' '}
            <Link to="/san-pham" className="text-brand-600 hover:underline">
              Mua sắm ngay
            </Link>
          </div>
        ) : (
          duLieu && (
            <>
              <ul className={`space-y-3 ${dangTai ? 'opacity-60' : ''}`}>
                {duLieu.duLieu.map((d) => (
                  <li key={d.maDonHang}>
                    <Link
                      to={`/don-hang/${d.maDonHang}`}
                      className="flex items-center gap-4 rounded-2xl border border-gray-100 p-4 shadow-sm transition hover:shadow-md"
                    >
                      <ProductImage src={d.anhDaiDien} alt={d.tenSanPhamDau ?? 'Đơn hàng'} className="h-16 w-16 shrink-0 rounded-xl text-xl" />
                      <div className="min-w-0 flex-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <span className="font-semibold text-gray-900">Đơn #{d.maDonHang}</span>
                          <OrderStatusBadge trangThai={d.trangThai} />
                        </div>
                        <p className="mt-1 line-clamp-1 text-sm text-gray-600">
                          {d.tenSanPhamDau}
                          {d.soMatHang > 1 && ` và ${d.soMatHang - 1} sản phẩm khác`}
                        </p>
                        <p className="text-xs text-gray-400">{dinhDangNgayGio(d.ngayDat)}</p>
                      </div>
                      <div className="text-right">
                        <p className="font-semibold text-brand-600">{dinhDangTien(d.tongTien)}</p>
                        <p className="text-xs text-gray-500">{d.tongSoLuong} sản phẩm</p>
                      </div>
                    </Link>
                  </li>
                ))}
              </ul>
              <div className="mt-6">
                <Pagination
                  trang={duLieu.trang}
                  tongSoTrang={duLieu.tongSoTrang}
                  onChange={(t) => setSearchParams({ trang: String(t) })}
                />
              </div>
            </>
          )
        )}
      </div>
    </div>
  )
}
