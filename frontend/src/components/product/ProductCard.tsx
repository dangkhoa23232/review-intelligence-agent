import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import { StarRating } from '@/components/common/StarRating'
import { useAuth } from '@/context/useAuth'
import { useCart } from '@/context/useCart'
import { gioHangApi } from '@/services/api/gioHangApi'
import type { SanPhamTomTat } from '@/types/sanPham'
import { dinhDangTien } from '@/utils/format'
import { thongBaoLoi } from '@/utils/useAsync'
import ProductImage from './ProductImage'

/** Thêm nhanh 1 sản phẩm của biến thể mặc định (còn hàng, rẻ nhất) vào giỏ. */
function NutThemNhanh({ sp }: { sp: SanPhamTomTat }) {
  const { nguoiDung } = useAuth()
  const { gioHang, datGioHang } = useCart()
  const navigate = useNavigate()
  const location = useLocation()
  const [dangThem, setDangThem] = useState(false)
  const [thongBao, setThongBao] = useState<{ loai: 'ok' | 'loi'; noiDung: string } | null>(null)

  useEffect(() => {
    if (!thongBao) return
    const t = window.setTimeout(() => setThongBao(null), 3000)
    return () => window.clearTimeout(t)
  }, [thongBao])

  const bt = sp.bienTheMacDinh
  if (nguoiDung?.vaiTro === 'QuanTri') return null

  const daCo = bt ? (gioHang?.danhSach.find((d) => d.maBienThe === bt.maBienThe)?.soLuong ?? 0) : 0
  const hetHang = !bt || bt.soLuongTon <= 0 || (nguoiDung != null && daCo >= bt.soLuongTon)

  async function them() {
    if (!bt) return
    if (!nguoiDung) {
      navigate(`/dang-nhap?tiepTuc=${encodeURIComponent(location.pathname + location.search)}`)
      return
    }
    setDangThem(true)
    try {
      datGioHang(await gioHangApi.them(bt.maBienThe, 1))
      setThongBao({ loai: 'ok', noiDung: `Đã thêm ${bt.dungTich} vào giỏ` })
    } catch (err) {
      setThongBao({ loai: 'loi', noiDung: thongBaoLoi(err) })
    } finally {
      setDangThem(false)
    }
  }

  return (
    <div className="mt-3">
      <button
        type="button"
        onClick={them}
        disabled={hetHang || dangThem}
        aria-label={bt ? `Thêm ${sp.ten} (${bt.dungTich}) vào giỏ hàng` : 'Hết hàng'}
        className="flex w-full items-center justify-center gap-1.5 rounded-full border border-brand-500 px-3 py-2 text-sm font-medium text-brand-600 transition-colors hover:bg-brand-500 hover:text-white disabled:cursor-not-allowed disabled:border-gray-200 disabled:text-gray-400 disabled:hover:bg-transparent"
      >
        {dangThem ? (
          'Đang thêm…'
        ) : hetHang ? (
          'Hết hàng'
        ) : (
          <>
            <svg viewBox="0 0 20 20" fill="currentColor" className="h-4 w-4" aria-hidden="true">
              <path d="M2.5 3a.75.75 0 0 0 0 1.5h1.2l1.7 8.1A1.75 1.75 0 0 0 7.1 14h7.6a1.75 1.75 0 0 0 1.7-1.3l1.3-5.4A.75.75 0 0 0 17 6.3H5.6l-.4-2A1.75 1.75 0 0 0 3.5 3h-1Zm5 13.25a1.25 1.25 0 1 0 0 2.5 1.25 1.25 0 0 0 0-2.5Zm7 0a1.25 1.25 0 1 0 0 2.5 1.25 1.25 0 0 0 0-2.5Z" />
            </svg>
            Thêm vào giỏ
            {sp.soBienThe > 1 && <span className="text-xs opacity-80">· {bt!.dungTich}</span>}
          </>
        )}
      </button>
      {thongBao && (
        <p
          role="status"
          className={`mt-1.5 text-center text-xs ${thongBao.loai === 'ok' ? 'text-green-700' : 'text-red-600'}`}
        >
          {thongBao.noiDung}
          {thongBao.loai === 'ok' && (
            <Link to="/gio-hang" className="ml-1 font-medium underline">
              Xem giỏ
            </Link>
          )}
        </p>
      )}
    </div>
  )
}

export default function ProductCard({ sp }: { sp: SanPhamTomTat }) {
  const lienKet = `/san-pham/${sp.duongDan}`
  const phanTramGiam =
    sp.giaGoc && sp.giaThapNhat !== null ? Math.round((1 - sp.giaThapNhat / sp.giaGoc) * 100) : null

  return (
    <article className="group flex flex-col overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-md">
      <Link to={lienKet} className="relative block overflow-hidden" tabIndex={-1} aria-hidden="true">
        <ProductImage
          src={sp.anhDaiDien}
          alt={sp.ten}
          className="aspect-square w-full transition-transform duration-300 group-hover:scale-[1.03]"
        />
        <div className="absolute left-2 top-2 flex flex-col gap-1">
          {phanTramGiam !== null && phanTramGiam > 0 && (
            <span className="rounded-full bg-brand-500 px-2 py-0.5 text-xs font-semibold text-white">-{phanTramGiam}%</span>
          )}
          {!sp.conHang && (
            <span className="rounded-full bg-gray-800/80 px-2 py-0.5 text-xs font-semibold text-white">Hết hàng</span>
          )}
        </div>
      </Link>

      <div className="flex flex-1 flex-col p-4">
        <p className="text-xs font-semibold uppercase tracking-wide text-brand-700">{sp.tenThuongHieu}</p>
        <h3 className="mt-1 line-clamp-2 min-h-[2.5rem] text-sm font-medium leading-5 text-gray-900">
          <Link to={lienKet} className="hover:text-brand-600">
            {sp.ten}
          </Link>
        </h3>

        <div className="mt-1.5 flex items-center gap-1.5 text-xs text-gray-500">
          <StarRating diem={sp.diemTrungBinh} kichThuoc={13} />
          <span>{sp.soDanhGia > 0 ? `(${sp.soDanhGia})` : 'Chưa có đánh giá'}</span>
        </div>

        <div className="mt-auto pt-3">
          {sp.giaThapNhat !== null ? (
            <p className="flex flex-wrap items-baseline gap-x-2">
              <span className="font-semibold text-brand-600">
                {sp.soBienThe > 1 && <span className="text-xs font-normal text-gray-500">Từ </span>}
                {dinhDangTien(sp.giaThapNhat)}
              </span>
              {sp.giaGoc !== null && (
                <span className="text-xs text-gray-400 line-through">{dinhDangTien(sp.giaGoc)}</span>
              )}
            </p>
          ) : (
            <p className="text-sm text-gray-500">Liên hệ</p>
          )}
          <NutThemNhanh sp={sp} />
        </div>
      </div>
    </article>
  )
}
