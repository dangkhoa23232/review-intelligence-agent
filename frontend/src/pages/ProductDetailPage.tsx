import { useEffect, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router'
import { StarRating } from '@/components/common/StarRating'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import ChatBox from '@/components/ai/ChatBox'
import AddToCart from '@/components/product/AddToCart'
import ProductImage from '@/components/product/ProductImage'
import AspectSummary from '@/components/review/AspectSummary'
import ReviewForm from '@/components/review/ReviewForm'
import ReviewItem from '@/components/review/ReviewItem'
import { sanPhamApi } from '@/services/api/sanPhamApi'
import type { DanhGia } from '@/types/danhGia'
import type { BienThe, SanPhamChiTiet } from '@/types/sanPham'
import { dinhDangNgay, dinhDangTien } from '@/utils/format'
import { thongBaoLoi, useAsync } from '@/utils/useAsync'

function TrangThaiKho({ bt }: { bt: BienThe }) {
  if (!bt.conHang) return <span className="text-sm font-medium text-gray-500">Hết hàng</span>
  if (bt.soLuongTon <= 10)
    return <span className="text-sm font-medium text-amber-600">Sắp hết – còn {bt.soLuongTon} sản phẩm</span>
  return <span className="text-sm text-green-700">Còn hàng ({bt.soLuongTon} sản phẩm)</span>
}

function KhoiGia({ bt }: { bt: BienThe }) {
  return (
    <div className="rounded-2xl bg-brand-50 p-4">
      <div className="flex flex-wrap items-baseline gap-3">
        <span className="text-3xl font-bold text-brand-600">{dinhDangTien(bt.giaApDung)}</span>
        {bt.giaKhuyenMai !== null && (
          <>
            <span className="text-gray-400 line-through">{dinhDangTien(bt.giaBan)}</span>
            <span className="rounded-full bg-brand-500 px-2 py-0.5 text-xs font-semibold text-white">
              -{Math.round((1 - bt.giaKhuyenMai / bt.giaBan) * 100)}%
            </span>
          </>
        )}
      </div>
      {bt.khuyenMaiDen && <p className="mt-1 text-xs text-brand-700">Khuyến mãi đến hết {dinhDangNgay(bt.khuyenMaiDen)}</p>}
      <p className="mt-2">
        <TrangThaiKho bt={bt} />
      </p>
      <p className="mt-1 text-xs text-gray-500">SKU: {bt.maSKU}</p>
    </div>
  )
}

function ThongTinMoTa({ sp }: { sp: SanPhamChiTiet }) {
  const muc = [
    { tieuDe: 'Công dụng', noiDung: sp.congDung },
    { tieuDe: 'Mô tả', noiDung: sp.moTa },
    { tieuDe: 'Thành phần', noiDung: sp.thanhPhan },
    { tieuDe: 'Hướng dẫn sử dụng', noiDung: sp.huongDanSuDung },
  ].filter((m) => m.noiDung)

  if (muc.length === 0) return null
  return (
    <dl className="space-y-4">
      {muc.map((m) => (
        <div key={m.tieuDe}>
          <dt className="text-sm font-semibold text-gray-900">{m.tieuDe}</dt>
          <dd className="mt-1 whitespace-pre-line text-sm text-gray-700">{m.noiDung}</dd>
        </div>
      ))}
    </dl>
  )
}

function DanhSachDanhGia({ sp }: { sp: SanPhamChiTiet }) {
  const [ds, setDs] = useState<DanhGia[]>(sp.danhGia.duLieu)
  const [trang, setTrang] = useState(sp.danhGia.trang)
  const [dangTai, setDangTai] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const conNua = trang < sp.danhGia.tongSoTrang

  async function xemThem() {
    setDangTai(true)
    setLoi(null)
    try {
      const kq = await sanPhamApi.danhGia(sp.duongDan, trang + 1, sp.danhGia.kichThuoc)
      setDs((cu) => [...cu, ...kq.duLieu])
      setTrang(kq.trang)
    } catch (err) {
      setLoi(thongBaoLoi(err))
    } finally {
      setDangTai(false)
    }
  }

  if (ds.length === 0) {
    return <p className="py-6 text-center text-sm text-gray-500">Chưa có đánh giá nào được duyệt. Hãy là người đầu tiên!</p>
  }

  return (
    <div>
      {ds.map((dg) => (
        <ReviewItem key={dg.maDanhGia} dg={dg} />
      ))}
      {loi && <p className="mt-2 text-sm text-red-600">{loi}</p>}
      {conNua && (
        <div className="mt-4 text-center">
          <button
            type="button"
            onClick={xemThem}
            disabled={dangTai}
            className="rounded-full border border-gray-300 px-5 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-60"
          >
            {dangTai ? 'Đang tải…' : `Xem thêm đánh giá (${sp.danhGia.tongSo - ds.length})`}
          </button>
        </div>
      )}
    </div>
  )
}

export default function ProductDetailPage() {
  const { duongDan = '' } = useParams()
  const { duLieu: sp, dangTai, loi, taiLai } = useAsync((signal) => sanPhamApi.chiTiet(duongDan, signal), [duongDan])
  const [maBienThe, setMaBienThe] = useState<number | null>(null)
  const [anhChon, setAnhChon] = useState(0)

  useEffect(() => {
    if (!sp) return
    // Mặc định chọn biến thể còn hàng rẻ nhất (API đã sắp theo giá).
    setMaBienThe((sp.bienThe.find((b) => b.conHang) ?? sp.bienThe[0])?.maBienThe ?? null)
    setAnhChon(0)
    document.title = `${sp.ten} – Review Intelligence`
  }, [sp])

  // Dữ liệu tải bất đồng bộ nên trình duyệt không tự cuộn tới #hash (ví dụ #viet-danh-gia) → cuộn thủ công.
  const { hash } = useLocation()
  useEffect(() => {
    if (!sp || !hash) return
    const id = window.setTimeout(() => {
      document.getElementById(decodeURIComponent(hash.slice(1)))?.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }, 50)
    return () => window.clearTimeout(id)
  }, [sp, hash])

  if (loi) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16">
        <HopLoi thongBao={loi} onThuLai={taiLai} />
        <p className="mt-4 text-center">
          <Link to="/san-pham" className="text-sm text-brand-600 hover:underline">
            ← Về danh sách sản phẩm
          </Link>
        </p>
      </div>
    )
  }
  if (dangTai || !sp) return <DangTai />

  const bienThe = sp.bienThe.find((b) => b.maBienThe === maBienThe) ?? null

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <nav className="text-sm text-gray-500" aria-label="Đường dẫn">
        <Link to="/san-pham" className="hover:text-brand-600">
          Sản phẩm
        </Link>
        <span className="mx-2">/</span>
        <Link to={`/san-pham?maDanhMuc=${sp.danhMuc.maDanhMuc}`} className="hover:text-brand-600">
          {sp.danhMuc.ten}
        </Link>
      </nav>

      <div className="mt-4 grid gap-10 md:grid-cols-2">
        {/* Ảnh */}
        <div>
          <ProductImage src={sp.danhSachAnh[anhChon]} alt={sp.ten} className="aspect-square w-full rounded-2xl" />
          {sp.danhSachAnh.length > 1 && (
            <div className="mt-3 flex gap-2">
              {sp.danhSachAnh.map((a, i) => (
                <button
                  key={a}
                  type="button"
                  onClick={() => setAnhChon(i)}
                  aria-label={`Ảnh ${i + 1}`}
                  className={`h-16 w-16 overflow-hidden rounded-lg ring-2 ${i === anhChon ? 'ring-brand-500' : 'ring-transparent'}`}
                >
                  <ProductImage src={a} alt={`${sp.ten} ${i + 1}`} className="h-full w-full text-base" />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Thông tin mua */}
        <div>
          <p className="text-sm font-medium uppercase tracking-wide text-gray-500">
            {sp.thuongHieu.ten}
            {sp.thuongHieu.quocGia && <span className="normal-case"> · {sp.thuongHieu.quocGia}</span>}
          </p>
          <h1 className="mt-1 text-2xl font-bold text-gray-900 sm:text-3xl">{sp.ten}</h1>

          <a href="#danh-gia" className="mt-2 inline-flex items-center gap-2 text-sm text-gray-600 hover:text-brand-600">
            <StarRating diem={sp.diemTrungBinh} />
            <span className="font-semibold text-gray-900">{sp.diemTrungBinh.toFixed(1)}</span>
            <span>({sp.soDanhGia} đánh giá)</span>
          </a>

          {sp.trangThai === 'NgungBan' && (
            <p className="mt-3 rounded-lg bg-gray-100 px-3 py-2 text-sm text-gray-700">Sản phẩm đã ngừng kinh doanh.</p>
          )}

          {sp.loaiDaPhuHop.length > 0 && (
            <div className="mt-4">
              <p className="text-sm font-medium text-gray-900">Phù hợp với</p>
              <div className="mt-2 flex flex-wrap gap-2">
                {sp.loaiDaPhuHop.map((l) => (
                  <Link
                    key={l.maLoaiDa}
                    to={`/san-pham?maLoaiDa=${l.maLoaiDa}`}
                    className="rounded-full bg-gray-100 px-3 py-1 text-xs text-gray-700 hover:bg-brand-50 hover:text-brand-700"
                  >
                    {l.ten}
                  </Link>
                ))}
              </div>
            </div>
          )}

          {sp.bienThe.length > 0 ? (
            <>
              <div className="mt-5">
                <p className="text-sm font-medium text-gray-900">Dung tích</p>
                <div className="mt-2 flex flex-wrap gap-2" role="radiogroup" aria-label="Chọn dung tích">
                  {sp.bienThe.map((b) => (
                    <button
                      key={b.maBienThe}
                      type="button"
                      role="radio"
                      aria-checked={b.maBienThe === maBienThe}
                      onClick={() => setMaBienThe(b.maBienThe)}
                      className={`rounded-xl border px-4 py-2 text-sm transition-colors ${
                        b.maBienThe === maBienThe
                          ? 'border-brand-500 bg-brand-50 font-semibold text-brand-700'
                          : 'border-gray-300 text-gray-700 hover:border-brand-300'
                      } ${b.conHang ? '' : 'opacity-50'}`}
                    >
                      {b.dungTich}
                      {!b.conHang && <span className="ml-1 text-xs">(hết)</span>}
                    </button>
                  ))}
                </div>
              </div>
              {bienThe && (
                <div className="mt-4 space-y-4">
                  <KhoiGia bt={bienThe} />
                  {sp.trangThai === 'DangBan' && <AddToCart bt={bienThe} tenSanPham={sp.ten} />}
                </div>
              )}
            </>
          ) : (
            <p className="mt-5 text-sm text-gray-500">Sản phẩm chưa có biến thể đang bán.</p>
          )}

          <div className="mt-6 border-t border-gray-100 pt-6">
            <ThongTinMoTa sp={sp} />
          </div>
        </div>
      </div>

      {/* Tóm tắt AI theo khía cạnh */}
      <section className="mt-14" aria-labelledby="tieu-de-tom-tat">
        <h2 id="tieu-de-tom-tat" className="text-xl font-bold text-gray-900">
          Tóm tắt đánh giá theo khía cạnh
        </h2>
        <p className="mt-1 text-sm text-gray-500">Tổng hợp tự động từ các đánh giá đã duyệt.</p>
        <div className="mt-4">
          <AspectSummary tomTat={sp.tomTatKhiaCanh} />
        </div>
      </section>

      {/* Hỏi đáp RAG có dẫn chứng */}
      <section className="mt-14" aria-label="Hỏi trợ lý AI">
        <ChatBox maSanPham={sp.maSanPham} tenSanPham={sp.ten} />
      </section>

      {/* Đánh giá */}
      <section id="danh-gia" className="mt-14 grid scroll-mt-24 gap-10 lg:grid-cols-[1fr_400px]">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Đánh giá từ khách hàng ({sp.danhGia.tongSo})</h2>
          <div className="mt-2">
            <DanhSachDanhGia key={sp.maSanPham} sp={sp} />
          </div>
        </div>
        <div id="viet-danh-gia" className="scroll-mt-24">
          <h2 className="text-xl font-bold text-gray-900">Viết đánh giá</h2>
          <div className="mt-4">
            <ReviewForm maSanPham={sp.maSanPham} />
          </div>
        </div>
      </section>
    </div>
  )
}
