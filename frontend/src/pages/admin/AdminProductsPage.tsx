import { useEffect, useState, type FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router'
import Pagination from '@/components/common/Pagination'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import ProductImage from '@/components/product/ProductImage'
import { adminSanPhamApi } from '@/services/api/sanPhamApi'
import type { SanPhamQuanTri, TrangThaiSanPham } from '@/types/sanPham'
import { dinhDangTien } from '@/utils/format'
import { thongBaoLoi, useAsync } from '@/utils/useAsync'

const KICH_THUOC_TRANG = 15
const CAC_TAB: { giaTri: TrangThaiSanPham | ''; nhan: string }[] = [
  { giaTri: '', nhan: 'Tất cả' },
  { giaTri: 'DangBan', nhan: 'Đang bán' },
  { giaTri: 'NgungBan', nhan: 'Ngừng bán' },
]

const NHAN_TRANG_THAI_SP: Record<TrangThaiSanPham, string> = { DangBan: 'Đang bán', NgungBan: 'Ngừng bán' }

function NhanTrangThai({ tt }: { tt: TrangThaiSanPham }) {
  return (
    <span
      className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${
        tt === 'DangBan' ? 'bg-green-50 text-green-700 ring-green-600/20' : 'bg-gray-100 text-gray-600 ring-gray-500/20'
      }`}
    >
      {NHAN_TRANG_THAI_SP[tt]}
    </span>
  )
}

export default function AdminProductsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const tt = searchParams.get('trangThai')
  const trangThai: TrangThaiSanPham | '' = tt === 'DangBan' || tt === 'NgungBan' ? tt : ''
  const tuKhoa = searchParams.get('tuKhoa') ?? ''
  const trang = Math.max(1, Number(searchParams.get('trang')) || 1)

  const [nhap, setNhap] = useState(tuKhoa)
  const [dangXuLy, setDangXuLy] = useState<number | null>(null)
  const [thongBao, setThongBao] = useState<{ loai: 'ok' | 'loi'; noiDung: string } | null>(null)
  useEffect(() => setNhap(tuKhoa), [tuKhoa])

  const { duLieu, dangTai, loi, taiLai } = useAsync(
    (signal) =>
      adminSanPhamApi.danhSach(
        { trangThai: trangThai || undefined, tuKhoa: tuKhoa || undefined, trang, kichThuoc: KICH_THUOC_TRANG },
        signal,
      ),
    [trangThai, tuKhoa, trang],
  )

  function datThamSo(moi: Record<string, string>) {
    const sp = new URLSearchParams()
    const gop = { trangThai, tuKhoa, ...moi }
    for (const [k, v] of Object.entries(gop)) if (v) sp.set(k, v)
    setSearchParams(sp)
  }

  function timKiem(e: FormEvent) {
    e.preventDefault()
    datThamSo({ tuKhoa: nhap.trim(), trang: '' })
  }

  async function doiTrangThai(sp: SanPhamQuanTri) {
    const moi: TrangThaiSanPham = sp.trangThai === 'DangBan' ? 'NgungBan' : 'DangBan'
    setDangXuLy(sp.maSanPham)
    setThongBao(null)
    try {
      await adminSanPhamApi.doiTrangThai(sp.maSanPham, moi)
      setThongBao({ loai: 'ok', noiDung: `“${sp.ten}” → ${NHAN_TRANG_THAI_SP[moi]}.` })
      taiLai()
    } catch (err) {
      setThongBao({ loai: 'loi', noiDung: thongBaoLoi(err) })
    } finally {
      setDangXuLy(null)
    }
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Sản phẩm</h1>
          <p className="mt-1 text-sm text-gray-500">Quản lý thông tin, ảnh, biến thể dung tích và lô hàng nhập kho.</p>
        </div>
        <Link
          to="/admin/san-pham/moi"
          className="rounded-full bg-brand-500 px-5 py-2.5 text-sm font-medium text-white hover:bg-brand-600"
        >
          + Thêm sản phẩm
        </Link>
      </div>

      <div className="mt-6 flex flex-wrap items-center justify-between gap-3 border-b border-gray-200">
        <div className="flex gap-1" role="tablist">
          {CAC_TAB.map((t) => (
            <button
              key={t.nhan}
              type="button"
              role="tab"
              aria-selected={t.giaTri === trangThai}
              onClick={() => datThamSo({ trangThai: t.giaTri, trang: '' })}
              className={`-mb-px border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
                t.giaTri === trangThai
                  ? 'border-brand-500 text-brand-600'
                  : 'border-transparent text-gray-500 hover:text-gray-800'
              }`}
            >
              {t.nhan}
              {duLieu && t.giaTri && (
                <span className="ml-2 rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">
                  {duLieu.soLuongTheoTrangThai[t.giaTri] ?? 0}
                </span>
              )}
            </button>
          ))}
        </div>
        <form onSubmit={timKiem} className="mb-2 flex gap-2">
          <input
            type="search"
            value={nhap}
            onChange={(e) => setNhap(e.target.value)}
            placeholder="Tên, đường dẫn hoặc SKU…"
            maxLength={100}
            className="w-56 rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-brand-500 focus:outline-none"
          />
          <button type="submit" className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm hover:bg-gray-50">
            Tìm
          </button>
        </form>
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

      <div className="mt-4">
        {loi ? (
          <HopLoi thongBao={loi} onThuLai={taiLai} />
        ) : dangTai && !duLieu ? (
          <DangTai />
        ) : duLieu && duLieu.ketQua.duLieu.length === 0 ? (
          <p className="rounded-2xl bg-gray-50 px-4 py-16 text-center text-sm text-gray-500">Không có sản phẩm phù hợp.</p>
        ) : (
          duLieu && (
            <div className={dangTai ? 'opacity-60' : ''}>
              <div className="overflow-x-auto rounded-2xl border border-gray-100">
                <table className="w-full min-w-[760px] text-left text-sm">
                  <thead className="bg-gray-50 text-xs uppercase tracking-wide text-gray-500">
                    <tr>
                      <th className="px-4 py-3 font-medium">Sản phẩm</th>
                      <th className="px-4 py-3 font-medium">Danh mục</th>
                      <th className="px-4 py-3 text-right font-medium">Giá từ</th>
                      <th className="px-4 py-3 text-right font-medium">Biến thể</th>
                      <th className="px-4 py-3 text-right font-medium">Tồn kho</th>
                      <th className="px-4 py-3 font-medium">Trạng thái</th>
                      <th className="px-4 py-3" />
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {duLieu.ketQua.duLieu.map((sp) => (
                      <tr key={sp.maSanPham} className="hover:bg-gray-50/60">
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-3">
                            <ProductImage src={sp.anhDaiDien} alt={sp.ten} className="h-12 w-12 shrink-0 rounded-lg text-base" />
                            <div className="min-w-0">
                              <Link
                                to={`/admin/san-pham/${sp.maSanPham}`}
                                className="line-clamp-1 font-medium text-gray-900 hover:text-brand-600"
                              >
                                {sp.ten}
                              </Link>
                              <p className="text-xs text-gray-500">
                                {sp.tenThuongHieu} · #{sp.maSanPham}
                              </p>
                            </div>
                          </div>
                        </td>
                        <td className="px-4 py-3 text-gray-600">{sp.tenDanhMuc}</td>
                        <td className="px-4 py-3 text-right tabular-nums">
                          {sp.giaThapNhat !== null ? dinhDangTien(sp.giaThapNhat) : '—'}
                        </td>
                        <td className="px-4 py-3 text-right tabular-nums">{sp.soBienThe}</td>
                        <td
                          className={`px-4 py-3 text-right tabular-nums ${sp.tongTon === 0 ? 'font-medium text-red-600' : sp.tongTon <= 10 ? 'text-amber-700' : ''}`}
                        >
                          {sp.tongTon}
                        </td>
                        <td className="px-4 py-3">
                          <NhanTrangThai tt={sp.trangThai} />
                        </td>
                        <td className="px-4 py-3 text-right">
                          <div className="flex justify-end gap-2">
                            <Link
                              to={`/admin/san-pham/${sp.maSanPham}`}
                              className="rounded-lg border border-gray-300 px-3 py-1 text-xs font-medium text-gray-700 hover:bg-gray-50"
                            >
                              Sửa
                            </Link>
                            <button
                              type="button"
                              disabled={dangXuLy === sp.maSanPham}
                              onClick={() => doiTrangThai(sp)}
                              className="rounded-lg border border-gray-300 px-3 py-1 text-xs font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-60"
                            >
                              {sp.trangThai === 'DangBan' ? 'Ngừng bán' : 'Mở bán'}
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <div className="pt-6">
                <Pagination
                  trang={duLieu.ketQua.trang}
                  tongSoTrang={duLieu.ketQua.tongSoTrang}
                  onChange={(p) => datThamSo({ trang: String(p) })}
                />
              </div>
            </div>
          )
        )}
      </div>
    </div>
  )
}
