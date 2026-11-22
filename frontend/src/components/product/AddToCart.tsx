import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router'
import QuantityStepper from '@/components/common/QuantityStepper'
import { useAuth } from '@/context/useAuth'
import { useCart } from '@/context/useCart'
import { gioHangApi } from '@/services/api/gioHangApi'
import type { BienThe } from '@/types/sanPham'
import { thongBaoLoi } from '@/utils/useAsync'

/** Chọn số lượng + thêm biến thể đang chọn vào giỏ (POST /api/gio-hang). */
export default function AddToCart({ bt, tenSanPham }: { bt: BienThe; tenSanPham: string }) {
  const { nguoiDung } = useAuth()
  const { gioHang, datGioHang } = useCart()
  const navigate = useNavigate()
  const location = useLocation()

  const [soLuong, setSoLuong] = useState(1)
  const [dangThem, setDangThem] = useState(false)
  const [thongBao, setThongBao] = useState<{ loai: 'ok' | 'loi'; noiDung: string } | null>(null)

  // Số lượng đã có trong giỏ → giới hạn còn thêm được.
  const daCo = gioHang?.danhSach.find((d) => d.maBienThe === bt.maBienThe)?.soLuong ?? 0
  const conThemDuoc = Math.max(0, bt.soLuongTon - daCo)

  useEffect(() => {
    setSoLuong(1)
    setThongBao(null)
  }, [bt.maBienThe])

  useEffect(() => {
    if (!thongBao || thongBao.loai !== 'ok') return
    const t = window.setTimeout(() => setThongBao(null), 4000)
    return () => window.clearTimeout(t)
  }, [thongBao])

  if (nguoiDung?.vaiTro === 'QuanTri') {
    return <p className="text-sm text-gray-500">Tài khoản quản trị không thể mua hàng.</p>
  }

  async function them() {
    if (!nguoiDung) {
      navigate(`/dang-nhap?tiepTuc=${encodeURIComponent(location.pathname)}`)
      return
    }
    setDangThem(true)
    setThongBao(null)
    try {
      datGioHang(await gioHangApi.them(bt.maBienThe, soLuong))
      setThongBao({ loai: 'ok', noiDung: `Đã thêm ${soLuong} × ${tenSanPham} (${bt.dungTich}) vào giỏ hàng.` })
      setSoLuong(1)
    } catch (err) {
      setThongBao({ loai: 'loi', noiDung: thongBaoLoi(err) })
    } finally {
      setDangThem(false)
    }
  }

  const hetHang = !bt.conHang || (nguoiDung && conThemDuoc === 0)

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-center gap-3">
        <QuantityStepper
          giaTri={soLuong}
          toiDa={Math.max(1, nguoiDung ? conThemDuoc : bt.soLuongTon)}
          onChange={setSoLuong}
          disabled={!!hetHang || dangThem}
        />
        <button
          type="button"
          onClick={them}
          disabled={!!hetHang || dangThem}
          className="flex-1 rounded-full bg-brand-500 px-6 py-3 font-medium text-white hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-50 sm:flex-none"
        >
          {dangThem ? 'Đang thêm…' : !bt.conHang ? 'Hết hàng' : 'Thêm vào giỏ hàng'}
        </button>
      </div>

      {nguoiDung && daCo > 0 && bt.conHang && (
        <p className="text-xs text-gray-500">
          Giỏ hàng đã có {daCo} sản phẩm này{conThemDuoc === 0 ? ' – đã đạt số lượng tồn kho tối đa.' : '.'}
        </p>
      )}

      {thongBao && (
        <p
          role="status"
          className={`rounded-lg px-3 py-2 text-sm ${thongBao.loai === 'ok' ? 'bg-green-50 text-green-800' : 'bg-red-50 text-red-700'}`}
        >
          {thongBao.noiDung}
          {thongBao.loai === 'ok' && (
            <Link to="/gio-hang" className="ml-2 font-medium underline">
              Xem giỏ hàng
            </Link>
          )}
        </p>
      )}
    </div>
  )
}
