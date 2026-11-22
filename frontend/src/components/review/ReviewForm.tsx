import { useEffect, useState, type FormEvent } from 'react'
import { Link, useLocation } from 'react-router'
import { StarInput } from '@/components/common/StarRating'
import { useAuth } from '@/context/useAuth'
import { danhGiaApi } from '@/services/api/danhGiaApi'
import { traCuuApi } from '@/services/api/traCuuApi'
import type { DongDonCoTheDanhGia } from '@/types/danhGia'
import type { KhiaCanh } from '@/types/traCuu'
import { dinhDangNgay } from '@/utils/format'
import { thongBaoLoi } from '@/utils/useAsync'

const NOI_DUNG_TOI_THIEU = 10
const NOI_DUNG_TOI_DA = 2000

/** Form viết đánh giá: sao tổng (bắt buộc) + điểm từng tiêu chí (tùy chọn) + gắn đơn đã mua (tùy chọn). */
export default function ReviewForm({ maSanPham }: { maSanPham: number }) {
  const { nguoiDung } = useAuth()
  const location = useLocation()

  const [khiaCanh, setKhiaCanh] = useState<KhiaCanh[]>([])
  const [dongDon, setDongDon] = useState<DongDonCoTheDanhGia[]>([])
  const [soSao, setSoSao] = useState(0)
  const [diem, setDiem] = useState<Record<number, number>>({})
  const [noiDung, setNoiDung] = useState('')
  const [maChiTietDH, setMaChiTietDH] = useState<number | ''>('')
  const [loi, setLoi] = useState<string | null>(null)
  const [dangGui, setDangGui] = useState(false)
  const [daGui, setDaGui] = useState<{ daMuaHang: boolean } | null>(null)

  const laKhachHang = nguoiDung?.vaiTro === 'KhachHang'

  useEffect(() => {
    if (!laKhachHang) return
    traCuuApi.khiaCanh().then(setKhiaCanh).catch(() => setKhiaCanh([]))
    danhGiaApi
      .dongDonCoTheDanhGia(maSanPham)
      .then((ds) => {
        setDongDon(ds)
        if (ds.length > 0) setMaChiTietDH(ds[0].maChiTietDH)
      })
      .catch(() => setDongDon([]))
  }, [laKhachHang, maSanPham])

  if (!nguoiDung) {
    return (
      <div className="rounded-2xl bg-gray-50 p-6 text-center text-sm text-gray-600">
        <Link
          to={`/dang-nhap?tiepTuc=${encodeURIComponent(location.pathname + '#viet-danh-gia')}`}
          className="font-medium text-brand-600 hover:underline"
        >
          Đăng nhập
        </Link>{' '}
        để viết đánh giá cho sản phẩm này.
      </div>
    )
  }

  if (!laKhachHang) {
    return (
      <p className="rounded-2xl bg-gray-50 p-6 text-center text-sm text-gray-600">
        Tài khoản quản trị không thể viết đánh giá.
      </p>
    )
  }

  if (daGui) {
    return (
      <div className="rounded-2xl border border-green-100 bg-green-50 p-6 text-center text-sm text-green-800" role="status">
        <p className="font-semibold">Cảm ơn bạn đã đánh giá!</p>
        <p className="mt-1">
          Đánh giá {daGui.daMuaHang ? 'kèm xác thực đã mua hàng ' : ''}đang chờ quản trị viên duyệt trước khi hiển thị.
        </p>
      </div>
    )
  }

  const doiDiem = (ma: number, v: number) =>
    setDiem((cu) => {
      const moi = { ...cu }
      if (moi[ma] === v) delete moi[ma] // bấm lại cùng số sao để bỏ chấm
      else moi[ma] = v
      return moi
    })

  async function gui(e: FormEvent) {
    e.preventDefault()
    const nd = noiDung.trim()
    if (soSao < 1) return setLoi('Vui lòng chọn số sao.')
    if (nd.length < NOI_DUNG_TOI_THIEU || nd.length > NOI_DUNG_TOI_DA)
      return setLoi(`Nội dung đánh giá phải từ ${NOI_DUNG_TOI_THIEU} đến ${NOI_DUNG_TOI_DA} ký tự.`)

    setLoi(null)
    setDangGui(true)
    try {
      const kq = await danhGiaApi.tao({
        maSanPham,
        soSao,
        noiDung: nd,
        maChiTietDH: maChiTietDH === '' ? undefined : maChiTietDH,
        diemTieuChi: Object.entries(diem).map(([ma, d]) => ({ maKhiaCanh: Number(ma), diem: d })),
      })
      setDaGui({ daMuaHang: kq.daMuaHang })
    } catch (err) {
      setLoi(thongBaoLoi(err))
    } finally {
      setDangGui(false)
    }
  }

  return (
    <form onSubmit={gui} className="space-y-5 rounded-2xl border border-gray-100 p-5" noValidate>
      <div>
        <p className="text-sm font-medium text-gray-900">
          Đánh giá chung <span className="text-red-500">*</span>
        </p>
        <div className="mt-1">
          <StarInput giaTri={soSao} onChange={setSoSao} nhan="Số sao đánh giá chung" />
        </div>
      </div>

      {khiaCanh.length > 0 && (
        <div>
          <p className="text-sm font-medium text-gray-900">
            Chấm điểm theo tiêu chí <span className="font-normal text-gray-500">(tùy chọn)</span>
          </p>
          <div className="mt-2 grid gap-x-6 gap-y-3 sm:grid-cols-2">
            {khiaCanh.map((k) => (
              <div key={k.maKhiaCanh} className="flex items-center justify-between gap-3">
                <span className="text-sm text-gray-700" title={k.moTa ?? undefined}>
                  {k.ten}
                </span>
                <StarInput
                  giaTri={diem[k.maKhiaCanh] ?? 0}
                  onChange={(v) => doiDiem(k.maKhiaCanh, v)}
                  kichThuoc={20}
                  nhan={`Điểm ${k.ten}`}
                />
              </div>
            ))}
          </div>
        </div>
      )}

      {dongDon.length > 0 && (
        <label className="block">
          <span className="text-sm font-medium text-gray-900">Gắn với đơn hàng đã mua</span>
          <select
            value={maChiTietDH}
            onChange={(e) => setMaChiTietDH(e.target.value === '' ? '' : Number(e.target.value))}
            className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-500 focus:outline-none"
          >
            {dongDon.map((d) => (
              <option key={d.maChiTietDH} value={d.maChiTietDH}>
                Đơn #{d.maDonHang} · {d.dungTich} · {dinhDangNgay(d.ngayDat)}
              </option>
            ))}
            <option value="">Không gắn đơn hàng</option>
          </select>
          <span className="mt-1 block text-xs text-gray-500">Đánh giá gắn đơn sẽ có huy hiệu “Đã mua hàng”.</span>
        </label>
      )}

      <label className="block">
        <span className="text-sm font-medium text-gray-900">
          Nội dung <span className="text-red-500">*</span>
        </span>
        <textarea
          rows={5}
          value={noiDung}
          maxLength={NOI_DUNG_TOI_DA}
          onChange={(e) => setNoiDung(e.target.value)}
          placeholder="Chia sẻ cảm nhận về kết cấu, mùi hương, độ thấm, hiệu quả…"
          className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-500 focus:outline-none"
        />
        <span className="mt-1 block text-right text-xs text-gray-400">
          {noiDung.trim().length}/{NOI_DUNG_TOI_DA}
        </span>
      </label>

      {loi && (
        <p className="text-sm text-red-600" role="alert">
          {loi}
        </p>
      )}

      <button
        type="submit"
        disabled={dangGui}
        className="rounded-full bg-brand-500 px-6 py-2.5 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-60"
      >
        {dangGui ? 'Đang gửi…' : 'Gửi đánh giá'}
      </button>
    </form>
  )
}
