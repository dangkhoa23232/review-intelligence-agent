import { Link } from 'react-router'
import ApiStatusBadge from '@/components/common/ApiStatusBadge'
import { healthApi } from '@/services/api/healthApi'

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
