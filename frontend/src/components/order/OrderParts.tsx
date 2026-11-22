import { Link } from 'react-router'
import ProductImage from '@/components/product/ProductImage'
import type { DongDonHang, TrangThaiDonHang } from '@/types/donHang'
import { MAU_TRANG_THAI_DON, NHAN_TRANG_THAI_DON, dinhDangTien } from '@/utils/format'

export function OrderStatusBadge({ trangThai }: { trangThai: TrangThaiDonHang }) {
  return (
    <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${MAU_TRANG_THAI_DON[trangThai]}`}>
      {NHAN_TRANG_THAI_DON[trangThai]}
    </span>
  )
}

/** Danh sách sản phẩm trong đơn (đã gộp theo biến thể). */
export function OrderItems({ dong, hienNutDanhGia = false }: { dong: DongDonHang[]; hienNutDanhGia?: boolean }) {
  return (
    <ul className="divide-y divide-gray-100">
      {dong.map((d) => (
        <li key={`${d.maBienThe}-${d.donGia}`} className="flex gap-3 py-3">
          <ProductImage src={d.anhDaiDien} alt={d.tenSanPham} className="h-16 w-16 shrink-0 rounded-lg text-xl" />
          <div className="min-w-0 flex-1">
            <Link to={`/san-pham/${d.duongDan}`} className="line-clamp-1 text-sm font-medium text-gray-900 hover:text-brand-600">
              {d.tenSanPham}
            </Link>
            <p className="text-xs text-gray-500">
              {d.dungTich} · {d.maSKU}
            </p>
            <p className="mt-1 text-xs text-gray-600">
              {dinhDangTien(d.donGia)} × {d.soLuong}
            </p>
            {hienNutDanhGia && d.coTheDanhGia && (
              <Link
                to={`/san-pham/${d.duongDan}#viet-danh-gia`}
                className="mt-1 inline-block text-xs font-medium text-brand-600 hover:underline"
              >
                Viết đánh giá ›
              </Link>
            )}
          </div>
          <p className="text-sm font-semibold text-gray-900">{dinhDangTien(d.thanhTien)}</p>
        </li>
      ))}
    </ul>
  )
}

/** Bảng tổng tiền: tiền hàng / giảm giá / phí ship / tổng cộng. */
export function OrderTotals({
  tienHang,
  tienGiam,
  phiVanChuyen,
  maCode,
}: {
  tienHang: number
  tienGiam: number
  phiVanChuyen: number
  maCode?: string | null
}) {
  const tong = tienHang - tienGiam + phiVanChuyen
  return (
    <dl className="space-y-2 text-sm">
      <div className="flex justify-between">
        <dt className="text-gray-600">Tiền hàng</dt>
        <dd className="text-gray-900">{dinhDangTien(tienHang)}</dd>
      </div>
      <div className="flex justify-between">
        <dt className="text-gray-600">Giảm giá{maCode ? ` (${maCode})` : ''}</dt>
        <dd className={tienGiam > 0 ? 'text-green-700' : 'text-gray-900'}>
          {tienGiam > 0 ? `−${dinhDangTien(tienGiam)}` : dinhDangTien(0)}
        </dd>
      </div>
      <div className="flex justify-between">
        <dt className="text-gray-600">Phí vận chuyển</dt>
        <dd className="text-gray-900">{dinhDangTien(phiVanChuyen)}</dd>
      </div>
      <div className="flex justify-between border-t border-gray-100 pt-2 text-base font-semibold">
        <dt className="text-gray-900">Tổng cộng</dt>
        <dd className="text-brand-600">{dinhDangTien(tong)}</dd>
      </div>
    </dl>
  )
}
