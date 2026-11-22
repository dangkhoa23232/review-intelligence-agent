import { Link } from 'react-router'
import { StarRating } from '@/components/common/StarRating'
import type { SanPhamTomTat } from '@/types/sanPham'
import { dinhDangTien } from '@/utils/format'
import ProductImage from './ProductImage'

export default function ProductCard({ sp }: { sp: SanPhamTomTat }) {
  return (
    <Link
      to={`/san-pham/${sp.duongDan}`}
      className="group flex flex-col overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
    >
      <div className="relative">
        <ProductImage src={sp.anhDaiDien} alt={sp.ten} className="aspect-square w-full" />
        <div className="absolute left-2 top-2 flex flex-col gap-1">
          {sp.coKhuyenMai && (
            <span className="rounded-full bg-brand-500 px-2 py-0.5 text-xs font-semibold text-white">Khuyến mãi</span>
          )}
          {!sp.conHang && (
            <span className="rounded-full bg-gray-800/80 px-2 py-0.5 text-xs font-semibold text-white">Hết hàng</span>
          )}
        </div>
      </div>

      <div className="flex flex-1 flex-col gap-1 p-4">
        <p className="text-xs font-medium uppercase tracking-wide text-gray-500">{sp.tenThuongHieu}</p>
        <h3 className="line-clamp-2 font-medium text-gray-900 group-hover:text-brand-600">{sp.ten}</h3>
        {sp.congDung && <p className="line-clamp-1 text-xs text-gray-500">{sp.congDung}</p>}

        <div className="mt-1 flex items-center gap-1.5 text-xs text-gray-500">
          <StarRating diem={sp.diemTrungBinh} kichThuoc={14} />
          <span>{sp.soDanhGia > 0 ? `(${sp.soDanhGia})` : 'Chưa có đánh giá'}</span>
        </div>

        <p className="mt-auto pt-2 font-semibold text-brand-600">
          {sp.giaThapNhat !== null ? (
            <>
              <span className="text-xs font-normal text-gray-500">Từ </span>
              {dinhDangTien(sp.giaThapNhat)}
            </>
          ) : (
            <span className="text-sm font-normal text-gray-500">Liên hệ</span>
          )}
        </p>
      </div>
    </Link>
  )
}
