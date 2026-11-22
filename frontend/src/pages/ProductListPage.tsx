import { useEffect, useState, type FormEvent } from 'react'
import { useSearchParams } from 'react-router'
import Pagination from '@/components/common/Pagination'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import ProductCard from '@/components/product/ProductCard'
import ProductFilters, { type GiaTriLoc } from '@/components/product/ProductFilters'
import { sanPhamApi } from '@/services/api/sanPhamApi'
import { traCuuApi } from '@/services/api/traCuuApi'
import type { SapXepSanPham, TimKiemSanPhamParams } from '@/types/sanPham'
import type { DanhMuc, LoaiDa, ThuongHieu } from '@/types/traCuu'
import { useAsync } from '@/utils/useAsync'

const KICH_THUOC_TRANG = 12

const LUA_CHON_SAP_XEP: { giaTri: SapXepSanPham; nhan: string }[] = [
  { giaTri: 'moiNhat', nhan: 'Mới nhất' },
  { giaTri: 'giaTang', nhan: 'Giá tăng dần' },
  { giaTri: 'giaGiam', nhan: 'Giá giảm dần' },
  { giaTri: 'danhGiaCao', nhan: 'Đánh giá cao' },
]

const KHOA_LOC = ['maDanhMuc', 'maThuongHieu', 'maLoaiDa', 'giaTu', 'giaDen'] as const

function docSo(sp: URLSearchParams, khoa: string): number | undefined {
  const v = sp.get(khoa)
  if (v === null || v === '') return undefined
  const n = Number(v)
  return Number.isFinite(n) && n >= 0 ? n : undefined
}

/** Toàn bộ trạng thái tìm kiếm nằm trên URL → chia sẻ / quay lại trang giữ nguyên bộ lọc. */
function docThamSo(sp: URLSearchParams): Required<Pick<TimKiemSanPhamParams, 'sapXep' | 'trang'>> & TimKiemSanPhamParams {
  const sapXep = sp.get('sapXep') as SapXepSanPham | null
  return {
    tuKhoa: sp.get('tuKhoa') ?? undefined,
    maDanhMuc: docSo(sp, 'maDanhMuc'),
    maThuongHieu: docSo(sp, 'maThuongHieu'),
    maLoaiDa: docSo(sp, 'maLoaiDa'),
    giaTu: docSo(sp, 'giaTu'),
    giaDen: docSo(sp, 'giaDen'),
    sapXep: sapXep && LUA_CHON_SAP_XEP.some((x) => x.giaTri === sapXep) ? sapXep : 'moiNhat',
    trang: Math.max(1, docSo(sp, 'trang') ?? 1),
  }
}

export default function ProductListPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const thamSo = docThamSo(searchParams)
  const khoaTruyVan = searchParams.toString()

  const [tuKhoaNhap, setTuKhoaNhap] = useState(thamSo.tuKhoa ?? '')
  const [moBoLoc, setMoBoLoc] = useState(false)
  const [traCuu, setTraCuu] = useState<{ danhMuc: DanhMuc[]; thuongHieu: ThuongHieu[]; loaiDa: LoaiDa[] }>({
    danhMuc: [],
    thuongHieu: [],
    loaiDa: [],
  })

  useEffect(() => {
    Promise.all([traCuuApi.danhMuc(), traCuuApi.thuongHieu(), traCuuApi.loaiDa()])
      .then(([danhMuc, thuongHieu, loaiDa]) => setTraCuu({ danhMuc, thuongHieu, loaiDa }))
      .catch(() => {
        /* bộ lọc trống – danh sách sản phẩm vẫn hoạt động */
      })
  }, [])

  useEffect(() => setTuKhoaNhap(thamSo.tuKhoa ?? ''), [thamSo.tuKhoa])

  const { duLieu, dangTai, loi, taiLai } = useAsync(
    (signal) => sanPhamApi.timKiem({ ...thamSo, kichThuoc: KICH_THUOC_TRANG }, signal),
    [khoaTruyVan],
  )

  /** Cập nhật URL; đổi điều kiện lọc thì quay về trang 1. */
  function capNhat(thayDoi: Partial<TimKiemSanPhamParams>, giuTrang = false) {
    const sp = new URLSearchParams(searchParams)
    for (const [k, v] of Object.entries(thayDoi)) {
      if (v === undefined || v === '' || v === null) sp.delete(k)
      else sp.set(k, String(v))
    }
    if (!giuTrang) sp.delete('trang')
    if (sp.get('sapXep') === 'moiNhat') sp.delete('sapXep')
    setSearchParams(sp)
  }

  function timKiem(e: FormEvent) {
    e.preventDefault()
    capNhat({ tuKhoa: tuKhoaNhap.trim() || undefined })
  }

  const giaTriLoc: GiaTriLoc = Object.fromEntries(KHOA_LOC.map((k) => [k, thamSo[k]]))

  const boLoc = (
    <ProductFilters
      {...traCuu}
      giaTri={giaTriLoc}
      onDoi={(t) => capNhat(t)}
      onXoaTatCa={() => capNhat(Object.fromEntries(KHOA_LOC.map((k) => [k, undefined])))}
    />
  )

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900">Sản phẩm</h1>

      <form onSubmit={timKiem} className="mt-4 flex gap-2" role="search">
        <input
          type="search"
          value={tuKhoaNhap}
          onChange={(e) => setTuKhoaNhap(e.target.value)}
          maxLength={100}
          placeholder="Tìm theo tên hoặc công dụng (ví dụ: kiềm dầu, phục hồi)…"
          aria-label="Từ khóa tìm kiếm"
          className="min-w-0 flex-1 rounded-full border border-gray-300 px-4 py-2.5 text-sm focus:border-brand-500 focus:outline-none"
        />
        <button type="submit" className="rounded-full bg-brand-500 px-5 text-sm font-medium text-white hover:bg-brand-600">
          Tìm
        </button>
      </form>

      <div className="mt-6 grid gap-8 lg:grid-cols-[240px_1fr]">
        <aside className="hidden lg:block">{boLoc}</aside>

        <section>
          <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
            <p className="text-sm text-gray-600">
              {duLieu ? (
                <>
                  <span className="font-semibold text-gray-900">{duLieu.tongSo}</span> sản phẩm
                  {thamSo.tuKhoa && (
                    <>
                      {' '}
                      cho “<span className="font-medium">{thamSo.tuKhoa}</span>”
                    </>
                  )}
                </>
              ) : (
                ' '
              )}
            </p>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => setMoBoLoc((x) => !x)}
                aria-expanded={moBoLoc}
                className="rounded-full border border-gray-300 px-4 py-1.5 text-sm lg:hidden"
              >
                Bộ lọc
              </button>
              <label className="flex items-center gap-2 text-sm text-gray-600">
                Sắp xếp
                <select
                  value={thamSo.sapXep}
                  onChange={(e) => capNhat({ sapXep: e.target.value as SapXepSanPham })}
                  className="rounded-lg border border-gray-300 px-2 py-1.5 text-sm focus:border-brand-500 focus:outline-none"
                >
                  {LUA_CHON_SAP_XEP.map((x) => (
                    <option key={x.giaTri} value={x.giaTri}>
                      {x.nhan}
                    </option>
                  ))}
                </select>
              </label>
            </div>
          </div>

          {moBoLoc && <div className="mb-6 rounded-2xl border border-gray-100 p-4 lg:hidden">{boLoc}</div>}

          {loi ? (
            <HopLoi thongBao={loi} onThuLai={taiLai} />
          ) : dangTai && !duLieu ? (
            <DangTai />
          ) : duLieu && duLieu.duLieu.length === 0 ? (
            <div className="rounded-2xl bg-gray-50 px-4 py-16 text-center text-gray-500">
              Không tìm thấy sản phẩm phù hợp. Hãy thử bỏ bớt bộ lọc.
            </div>
          ) : (
            duLieu && (
              <>
                <div className={`grid grid-cols-2 gap-4 sm:grid-cols-3 ${dangTai ? 'opacity-60' : ''}`}>
                  {duLieu.duLieu.map((sp) => (
                    <ProductCard key={sp.maSanPham} sp={sp} />
                  ))}
                </div>
                <div className="mt-8">
                  <Pagination
                    trang={duLieu.trang}
                    tongSoTrang={duLieu.tongSoTrang}
                    onChange={(t) => {
                      capNhat({ trang: t }, true)
                      window.scrollTo({ top: 0, behavior: 'smooth' })
                    }}
                  />
                </div>
              </>
            )
          )}
        </section>
      </div>
    </div>
  )
}
