import { useState, type FormEvent } from 'react'
import { Link, useLocation } from 'react-router'
import { StarRating } from '@/components/common/StarRating'
import { useAuth } from '@/context/useAuth'
import { danhGiaApi } from '@/services/api/danhGiaApi'
import type { DanhGia } from '@/types/danhGia'
import { MAU_CAM_XUC, NHAN_CAM_XUC, dinhDangNgay } from '@/utils/format'
import { thongBaoLoi } from '@/utils/useAsync'

function FormBaoCao({ maDanhGia, onXong }: { maDanhGia: number; onXong: (thongBao: string) => void }) {
  const [lyDo, setLyDo] = useState('')
  const [loi, setLoi] = useState<string | null>(null)
  const [dangGui, setDangGui] = useState(false)

  async function gui(e: FormEvent) {
    e.preventDefault()
    const giaTri = lyDo.trim()
    if (giaTri.length < 5 || giaTri.length > 300) {
      setLoi('Lý do báo cáo phải từ 5 đến 300 ký tự.')
      return
    }
    setDangGui(true)
    setLoi(null)
    try {
      await danhGiaApi.baoCao(maDanhGia, giaTri)
      onXong('Đã gửi báo cáo. Cảm ơn bạn!')
    } catch (err) {
      setLoi(thongBaoLoi(err))
    } finally {
      setDangGui(false)
    }
  }

  return (
    <form onSubmit={gui} className="mt-3 rounded-lg bg-gray-50 p-3">
      <label className="block text-xs font-medium text-gray-700" htmlFor={`bao-cao-${maDanhGia}`}>
        Lý do báo cáo
      </label>
      <textarea
        id={`bao-cao-${maDanhGia}`}
        rows={2}
        maxLength={300}
        value={lyDo}
        onChange={(e) => setLyDo(e.target.value)}
        placeholder="Ví dụ: nội dung quảng cáo, ngôn từ không phù hợp…"
        className="mt-1 w-full rounded-lg border border-gray-300 px-2 py-1.5 text-sm focus:border-brand-500 focus:outline-none"
      />
      {loi && <p className="mt-1 text-xs text-red-600">{loi}</p>}
      <button
        type="submit"
        disabled={dangGui}
        className="mt-2 rounded-lg bg-gray-800 px-3 py-1.5 text-xs font-medium text-white hover:bg-gray-900 disabled:opacity-60"
      >
        {dangGui ? 'Đang gửi…' : 'Gửi báo cáo'}
      </button>
    </form>
  )
}

export default function ReviewItem({ dg }: { dg: DanhGia }) {
  const { nguoiDung } = useAuth()
  const location = useLocation()
  const [moBaoCao, setMoBaoCao] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)

  return (
    <article id={`danh-gia-${dg.maDanhGia}`} className="scroll-mt-24 border-b border-gray-100 py-5 last:border-0">
      <header className="flex flex-wrap items-center gap-x-3 gap-y-1">
        <span className="font-medium text-gray-900">{dg.tenNguoiDanhGia}</span>
        {dg.daMuaHang && (
          <span className="inline-flex items-center gap-1 rounded-full bg-green-50 px-2 py-0.5 text-xs font-medium text-green-700 ring-1 ring-green-600/20 ring-inset">
            ✓ Đã mua hàng{dg.dungTichDaMua ? ` · ${dg.dungTichDaMua}` : ''}
          </span>
        )}
        {dg.camXuc && (
          <span className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${MAU_CAM_XUC[dg.camXuc]}`}>
            {NHAN_CAM_XUC[dg.camXuc]}
          </span>
        )}
        <span className="ml-auto text-xs text-gray-400">{dinhDangNgay(dg.ngayTao)}</span>
      </header>

      <div className="mt-1">
        <StarRating diem={dg.soSao} kichThuoc={14} />
      </div>

      <p className="mt-2 whitespace-pre-line text-sm leading-relaxed text-gray-700">{dg.noiDung}</p>

      {dg.diemTieuChi.length > 0 && (
        <ul className="mt-3 flex flex-wrap gap-2">
          {dg.diemTieuChi.map((c) => (
            <li key={c.maKhiaCanh} className="rounded-md bg-gray-50 px-2 py-1 text-xs text-gray-600">
              {c.tenKhiaCanh}: <span className="font-semibold text-gray-900">{c.diem}/5</span>
            </li>
          ))}
        </ul>
      )}

      <div className="mt-2 text-xs">
        {thongBao ? (
          <span className="text-green-700">{thongBao}</span>
        ) : nguoiDung ? (
          <button type="button" onClick={() => setMoBaoCao((x) => !x)} className="text-gray-400 hover:text-gray-600">
            {moBaoCao ? 'Hủy báo cáo' : 'Báo cáo'}
          </button>
        ) : (
          <Link
            to={`/dang-nhap?tiepTuc=${encodeURIComponent(location.pathname)}`}
            className="text-gray-400 hover:text-gray-600"
          >
            Đăng nhập để báo cáo
          </Link>
        )}
      </div>

      {moBaoCao && !thongBao && (
        <FormBaoCao
          maDanhGia={dg.maDanhGia}
          onXong={(tb) => {
            setThongBao(tb)
            setMoBaoCao(false)
          }}
        />
      )}
    </article>
  )
}
