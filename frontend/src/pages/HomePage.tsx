import { Link } from 'react-router'
import ApiStatusBadge from '@/components/common/ApiStatusBadge'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import ProductCard from '@/components/product/ProductCard'
import { healthApi } from '@/services/api/healthApi'
import { sanPhamApi } from '@/services/api/sanPhamApi'
import type { SapXepSanPham } from '@/types/sanPham'
import { useAsync } from '@/utils/useAsync'

const features = [
  {
    title: 'Phân tích cảm xúc theo khía cạnh',
    desc: 'Đánh giá được tách theo từng khía cạnh như kết cấu, mùi hương, độ kích ứng, giá cả…',
  },
  {
    title: 'Tóm tắt ưu / nhược điểm',
    desc: 'AI tổng hợp hàng trăm đánh giá thành vài ý chính cho từng sản phẩm.',
  },
  {
    title: 'Hỏi đáp có dẫn chứng',
    desc: 'Đặt câu hỏi về sản phẩm, câu trả lời kèm trích dẫn review thật (RAG).',
  },
]

/** Lưới 8 thẻ sản phẩm lấy từ GET /api/san-pham. */
function KhuSanPham({ tieuDe, sapXep, xemThem }: { tieuDe: string; sapXep: SapXepSanPham; xemThem: string }) {
  const { duLieu, dangTai, loi, taiLai } = useAsync(
    (signal) => sanPhamApi.timKiem({ sapXep, trang: 1, kichThuoc: 8 }, signal),
    [sapXep],
  )

  return (
    <section className="mx-auto max-w-6xl px-4 py-10" aria-label={tieuDe}>
      <div className="flex items-end justify-between gap-4">
        <h2 className="text-2xl font-bold text-gray-900">{tieuDe}</h2>
        <Link to={xemThem} className="text-sm font-medium text-brand-600 hover:underline">
          Xem tất cả →
        </Link>
      </div>
      <div className="mt-6">
        {loi ? (
          <HopLoi thongBao={loi} onThuLai={taiLai} />
        ) : dangTai && !duLieu ? (
          <DangTai />
        ) : duLieu && duLieu.duLieu.length > 0 ? (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {duLieu.duLieu.map((sp) => (
              <ProductCard key={sp.maSanPham} sp={sp} />
            ))}
          </div>
        ) : (
          <p className="rounded-2xl bg-gray-50 px-4 py-10 text-center text-sm text-gray-500">Chưa có sản phẩm.</p>
        )}
      </div>
    </section>
  )
}

export default function HomePage() {
  return (
    <>
      <section className="bg-brand-50">
        <div className="mx-auto max-w-6xl px-4 py-20 text-center">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 sm:text-5xl">
            Chọn mỹ phẩm hợp làn da của bạn
          </h1>
          <p className="mx-auto mt-4 max-w-2xl text-lg text-gray-600">
            Đọc hiểu hàng nghìn đánh giá chỉ trong vài giây nhờ AI phân tích review.
          </p>
          <div className="mt-8 flex justify-center gap-3">
            <Link to="/san-pham" className="rounded-full bg-brand-500 px-6 py-3 font-medium text-white hover:bg-brand-600">
              Khám phá sản phẩm
            </Link>
          </div>
        </div>
      </section>

      <KhuSanPham tieuDe="Sản phẩm mới" sapXep="moiNhat" xemThem="/san-pham" />
      <KhuSanPham tieuDe="Được đánh giá cao" sapXep="danhGiaCao" xemThem="/san-pham?sapXep=danhGiaCao" />

      <section className="mx-auto grid max-w-6xl gap-6 px-4 py-16 sm:grid-cols-3">
        {features.map((f) => (
          <div key={f.title} className="rounded-2xl border border-gray-100 p-6 shadow-sm">
            <h2 className="font-semibold text-gray-900">{f.title}</h2>
            <p className="mt-2 text-sm text-gray-600">{f.desc}</p>
          </div>
        ))}
      </section>

      {import.meta.env.DEV && (
        <section className="mx-auto flex max-w-6xl flex-wrap gap-2 px-4 pb-10">
          <ApiStatusBadge name="Backend API" check={healthApi.backend} />
          <ApiStatusBadge name="AI service" check={healthApi.aiService} />
        </section>
      )}
    </>
  )
}
