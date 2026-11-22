import { useEffect, useState, type FormEvent, type ReactNode } from 'react'
import { Link, useNavigate } from 'react-router'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import { OrderTotals } from '@/components/order/OrderParts'
import ProductImage from '@/components/product/ProductImage'
import { useCart } from '@/context/useCart'
import { authApi } from '@/services/api/authApi'
import { donHangApi, phieuGiamGiaApi } from '@/services/api/donHangApi'
import type { PhuongThucGiaoHang, PhuongThucThanhToan } from '@/types/donHang'
import { NHAN_GIAO_HANG, NHAN_THANH_TOAN, dinhDangNgay, dinhDangTien } from '@/utils/format'
import { thongBaoLoi } from '@/utils/useAsync'

const SDT_HOP_LE = /^\+?\d{9,15}$/

type LoiForm = Partial<Record<'tenNguoiNhan' | 'soDienThoai' | 'diaChiGiaoHang', string>>

function kiemTraForm(f: { tenNguoiNhan: string; soDienThoai: string; diaChiGiaoHang: string }): LoiForm {
  const loi: LoiForm = {}
  const ten = f.tenNguoiNhan.trim()
  if (ten.length < 2 || ten.length > 150) loi.tenNguoiNhan = 'Tên người nhận phải từ 2 đến 150 ký tự.'
  if (!SDT_HOP_LE.test(f.soDienThoai.trim())) loi.soDienThoai = 'Số điện thoại không hợp lệ (9–15 chữ số).'
  const dc = f.diaChiGiaoHang.trim()
  if (dc.length < 5 || dc.length > 300) loi.diaChiGiaoHang = 'Địa chỉ giao hàng phải từ 5 đến 300 ký tự.'
  return loi
}

function TheChon({
  ten,
  giaTri,
  chon,
  onChon,
  children,
}: {
  ten: string
  giaTri: string
  chon: boolean
  onChon: () => void
  children: ReactNode
}) {
  return (
    <label
      className={`flex cursor-pointer items-center gap-3 rounded-xl border p-3 text-sm transition-colors ${
        chon ? 'border-brand-500 bg-brand-50' : 'border-gray-200 hover:border-brand-300'
      }`}
    >
      <input type="radio" name={ten} value={giaTri} checked={chon} onChange={onChon} className="accent-brand-500" />
      {children}
    </label>
  )
}

const inputCls =
  'mt-1 w-full rounded-lg border px-3 py-2 text-sm focus:border-brand-500 focus:outline-none aria-[invalid=true]:border-red-400'

export default function CheckoutPage() {
  const { gioHang, taiLai } = useCart()
  const navigate = useNavigate()

  const [dangTai, setDangTai] = useState(true)
  const [tenNguoiNhan, setTenNguoiNhan] = useState('')
  const [soDienThoai, setSoDienThoai] = useState('')
  const [diaChiGiaoHang, setDiaChiGiaoHang] = useState('')
  const [giaoHang, setGiaoHang] = useState<PhuongThucGiaoHang>('TieuChuan')
  const [thanhToan, setThanhToan] = useState<PhuongThucThanhToan>('TienMat')
  const [loiForm, setLoiForm] = useState<LoiForm>({})

  const [maNhap, setMaNhap] = useState('')
  const [ma, setMa] = useState<{ maCode: string; tienGiam: number; ketThuc: string } | null>(null)
  const [loiMa, setLoiMa] = useState<string | null>(null)
  const [dangKiemTraMa, setDangKiemTraMa] = useState(false)

  const [loi, setLoi] = useState<string | null>(null)
  const [dangDat, setDangDat] = useState(false)

  useEffect(() => {
    // Tải giỏ hàng mới nhất + tự điền thông tin nhận hàng từ hồ sơ (người dùng vẫn sửa được).
    Promise.all([taiLai(), authApi.toi().catch(() => null)])
      .then(([, hoSo]) => {
        if (!hoSo) return
        setTenNguoiNhan((v) => v || hoSo.hoTen)
        setSoDienThoai((v) => v || (hoSo.soDienThoai ?? ''))
        setDiaChiGiaoHang((v) => v || (hoSo.diaChi ?? ''))
      })
      .finally(() => setDangTai(false))
  }, [taiLai])

  if (dangTai && !gioHang) return <DangTai />
  if (!gioHang) return <HopLoi thongBao="Không tải được giỏ hàng." onThuLai={() => void taiLai()} />

  if (gioHang.danhSach.length === 0) {
    return (
      <div className="mx-auto max-w-xl px-4 py-24 text-center">
        <p className="text-gray-600">Giỏ hàng trống, chưa thể thanh toán.</p>
        <Link to="/san-pham" className="mt-4 inline-block text-brand-600 hover:underline">
          Tiếp tục mua sắm
        </Link>
      </div>
    )
  }

  const phiVanChuyen = gioHang.phiVanChuyen[giaoHang]
  const tienGiam = ma?.tienGiam ?? 0

  async function apDungMa() {
    const code = maNhap.trim()
    if (!code) return setLoiMa('Vui lòng nhập mã giảm giá.')
    setDangKiemTraMa(true)
    setLoiMa(null)
    try {
      const kq = await phieuGiamGiaApi.kiemTra(code, gioHang!.tamTinh)
      setMa({ maCode: kq.maCode, tienGiam: kq.tienGiam, ketThuc: kq.ketThuc })
      setMaNhap(kq.maCode)
    } catch (err) {
      setMa(null)
      setLoiMa(thongBaoLoi(err))
    } finally {
      setDangKiemTraMa(false)
    }
  }

  async function datHang(e: FormEvent) {
    e.preventDefault()
    const loiMoi = kiemTraForm({ tenNguoiNhan, soDienThoai, diaChiGiaoHang })
    setLoiForm(loiMoi)
    if (Object.keys(loiMoi).length > 0) return

    setDangDat(true)
    setLoi(null)
    try {
      const don = await donHangApi.datHang({
        tenNguoiNhan: tenNguoiNhan.trim(),
        soDienThoai: soDienThoai.trim(),
        diaChiGiaoHang: diaChiGiaoHang.trim(),
        phuongThucGiaoHang: giaoHang,
        phuongThucThanhToan: thanhToan,
        maCode: ma?.maCode,
      })
      await taiLai() // giỏ đã được xóa ở server → badge về 0
      navigate(`/don-hang/${don.maDonHang}`, { replace: true, state: { moiDat: true } })
    } catch (err) {
      setLoi(thongBaoLoi(err))
      void taiLai() // tồn kho / giá có thể đã đổi
    } finally {
      setDangDat(false)
    }
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">Thanh toán</h1>

      <form onSubmit={datHang} noValidate className="mt-6 grid gap-8 lg:grid-cols-[1fr_380px]">
        <div className="space-y-8">
          <section aria-labelledby="tt-nhan-hang">
            <h2 id="tt-nhan-hang" className="text-lg font-semibold text-gray-900">
              Thông tin nhận hàng
            </h2>
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              <label className="block">
                <span className="text-sm text-gray-700">Họ tên người nhận</span>
                <input
                  value={tenNguoiNhan}
                  onChange={(e) => setTenNguoiNhan(e.target.value)}
                  maxLength={150}
                  autoComplete="name"
                  aria-invalid={!!loiForm.tenNguoiNhan}
                  className={`${inputCls} border-gray-300`}
                />
                {loiForm.tenNguoiNhan && <span className="mt-1 block text-xs text-red-600">{loiForm.tenNguoiNhan}</span>}
              </label>
              <label className="block">
                <span className="text-sm text-gray-700">Số điện thoại</span>
                <input
                  value={soDienThoai}
                  onChange={(e) => setSoDienThoai(e.target.value)}
                  maxLength={20}
                  inputMode="tel"
                  autoComplete="tel"
                  aria-invalid={!!loiForm.soDienThoai}
                  className={`${inputCls} border-gray-300`}
                />
                {loiForm.soDienThoai && <span className="mt-1 block text-xs text-red-600">{loiForm.soDienThoai}</span>}
              </label>
              <label className="block sm:col-span-2">
                <span className="text-sm text-gray-700">Địa chỉ giao hàng</span>
                <textarea
                  rows={2}
                  value={diaChiGiaoHang}
                  onChange={(e) => setDiaChiGiaoHang(e.target.value)}
                  maxLength={300}
                  autoComplete="street-address"
                  aria-invalid={!!loiForm.diaChiGiaoHang}
                  className={`${inputCls} border-gray-300`}
                />
                {loiForm.diaChiGiaoHang && <span className="mt-1 block text-xs text-red-600">{loiForm.diaChiGiaoHang}</span>}
              </label>
            </div>
          </section>

          <section aria-labelledby="tt-giao-hang">
            <h2 id="tt-giao-hang" className="text-lg font-semibold text-gray-900">
              Phương thức giao hàng
            </h2>
            <div className="mt-3 grid gap-3 sm:grid-cols-2">
              {(Object.keys(NHAN_GIAO_HANG) as PhuongThucGiaoHang[]).map((pt) => (
                <TheChon key={pt} ten="giaoHang" giaTri={pt} chon={giaoHang === pt} onChon={() => setGiaoHang(pt)}>
                  <span className="flex-1 font-medium text-gray-900">{NHAN_GIAO_HANG[pt]}</span>
                  <span className="text-gray-700">{dinhDangTien(gioHang.phiVanChuyen[pt])}</span>
                </TheChon>
              ))}
            </div>
          </section>

          <section aria-labelledby="tt-thanh-toan">
            <h2 id="tt-thanh-toan" className="text-lg font-semibold text-gray-900">
              Phương thức thanh toán
            </h2>
            <div className="mt-3 grid gap-3">
              {(Object.keys(NHAN_THANH_TOAN) as PhuongThucThanhToan[]).map((pt) => (
                <TheChon key={pt} ten="thanhToan" giaTri={pt} chon={thanhToan === pt} onChon={() => setThanhToan(pt)}>
                  <span className="font-medium text-gray-900">{NHAN_THANH_TOAN[pt]}</span>
                </TheChon>
              ))}
            </div>
          </section>
        </div>

        <aside className="h-fit space-y-5 rounded-2xl bg-gray-50 p-5 lg:sticky lg:top-24">
          <h2 className="font-semibold text-gray-900">Đơn hàng ({gioHang.tongSoLuong} sản phẩm)</h2>
          <ul className="max-h-64 space-y-3 overflow-y-auto">
            {gioHang.danhSach.map((d) => (
              <li key={d.maBienThe} className="flex gap-3 text-sm">
                <div className="relative shrink-0">
                  <ProductImage src={d.anhDaiDien} alt={d.tenSanPham} className="h-12 w-12 rounded-lg text-base" />
                  <span className="absolute -right-1.5 -top-1.5 rounded-full bg-gray-700 px-1.5 text-[10px] font-semibold text-white">
                    {d.soLuong}
                  </span>
                </div>
                <div className="min-w-0 flex-1">
                  <p className="line-clamp-1 text-gray-900">{d.tenSanPham}</p>
                  <p className="text-xs text-gray-500">{d.dungTich}</p>
                </div>
                <span className="text-gray-900">{dinhDangTien(d.thanhTien)}</span>
              </li>
            ))}
          </ul>

          <div>
            <label htmlFor="ma-giam-gia" className="text-sm font-medium text-gray-900">
              Mã giảm giá
            </label>
            {ma ? (
              <div className="mt-1 flex items-center justify-between rounded-lg bg-green-50 px-3 py-2 text-sm text-green-800">
                <span>
                  <span className="font-semibold">{ma.maCode}</span> · −{dinhDangTien(ma.tienGiam)}
                  <span className="block text-xs text-green-700">Hạn đến {dinhDangNgay(ma.ketThuc)}</span>
                </span>
                <button type="button" onClick={() => setMa(null)} className="text-xs font-medium underline">
                  Bỏ mã
                </button>
              </div>
            ) : (
              <div className="mt-1 flex gap-2">
                <input
                  id="ma-giam-gia"
                  value={maNhap}
                  onChange={(e) => setMaNhap(e.target.value.toUpperCase())}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault()
                      void apDungMa()
                    }
                  }}
                  maxLength={30}
                  placeholder="Ví dụ: CHAOBAN10"
                  className="min-w-0 flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm uppercase focus:border-brand-500 focus:outline-none"
                />
                <button
                  type="button"
                  onClick={() => void apDungMa()}
                  disabled={dangKiemTraMa}
                  className="rounded-lg border border-brand-500 px-4 text-sm font-medium text-brand-600 hover:bg-brand-50 disabled:opacity-60"
                >
                  {dangKiemTraMa ? '…' : 'Áp dụng'}
                </button>
              </div>
            )}
            {loiMa && <p className="mt-1 text-xs text-red-600">{loiMa}</p>}
          </div>

          <OrderTotals tienHang={gioHang.tamTinh} tienGiam={tienGiam} phiVanChuyen={phiVanChuyen} maCode={ma?.maCode} />

          {!gioHang.coTheDatHang && (
            <p className="text-xs font-medium text-amber-700">
              Một số sản phẩm đã hết hàng hoặc vượt tồn kho.{' '}
              <Link to="/gio-hang" className="underline">
                Quay lại giỏ hàng
              </Link>
            </p>
          )}
          {loi && (
            <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
              {loi}
            </p>
          )}

          <button
            type="submit"
            disabled={dangDat || !gioHang.coTheDatHang}
            className="w-full rounded-full bg-brand-500 py-3 font-medium text-white hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {dangDat ? 'Đang đặt hàng…' : 'Xác nhận đặt hàng'}
          </button>
          <p className="text-center text-xs text-gray-500">Giá, tồn kho và mã giảm giá được kiểm tra lại khi xác nhận.</p>
        </aside>
      </form>
    </div>
  )
}
