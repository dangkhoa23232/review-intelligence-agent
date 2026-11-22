import { useEffect, useState, type FormEvent, type ReactNode } from 'react'
import type { DanhMuc, LoaiDa, ThuongHieu } from '@/types/traCuu'

export interface GiaTriLoc {
  maDanhMuc?: number
  maThuongHieu?: number
  maLoaiDa?: number
  giaTu?: number
  giaDen?: number
}

type KhoaLoc = keyof GiaTriLoc

const MUC_GIA: { nhan: string; tu?: number; den?: number }[] = [
  { nhan: 'Dưới 200.000đ', den: 200_000 },
  { nhan: '200.000đ – 400.000đ', tu: 200_000, den: 400_000 },
  { nhan: '400.000đ – 700.000đ', tu: 400_000, den: 700_000 },
  { nhan: 'Trên 700.000đ', tu: 700_000 },
]

function Nhom({ tieuDe, children }: { tieuDe: string; children: ReactNode }) {
  return (
    <fieldset className="border-b border-gray-100 py-4 first:pt-0 last:border-0">
      <legend className="mb-2 text-sm font-semibold text-gray-900">{tieuDe}</legend>
      <div className="space-y-1">{children}</div>
    </fieldset>
  )
}

function LuaChon({ chon, onClick, children, thutLe = false }: { chon: boolean; onClick: () => void; children: ReactNode; thutLe?: boolean }) {
  return (
    <button
      type="button"
      aria-pressed={chon}
      onClick={onClick}
      className={`flex w-full items-center gap-2 rounded-lg px-2 py-1.5 text-left text-sm transition-colors ${
        thutLe ? 'pl-6' : ''
      } ${chon ? 'bg-brand-50 font-medium text-brand-700' : 'text-gray-600 hover:bg-gray-50'}`}
    >
      <span
        className={`h-3.5 w-3.5 shrink-0 rounded-full border ${chon ? 'border-brand-500 bg-brand-500 ring-2 ring-brand-100' : 'border-gray-300'}`}
      />
      {children}
    </button>
  )
}

export default function ProductFilters({
  danhMuc,
  thuongHieu,
  loaiDa,
  giaTri,
  onDoi,
  onXoaTatCa,
}: {
  danhMuc: DanhMuc[]
  thuongHieu: ThuongHieu[]
  loaiDa: LoaiDa[]
  giaTri: GiaTriLoc
  onDoi: (thayDoi: Partial<GiaTriLoc>) => void
  onXoaTatCa: () => void
}) {
  const [giaTu, setGiaTu] = useState(giaTri.giaTu?.toString() ?? '')
  const [giaDen, setGiaDen] = useState(giaTri.giaDen?.toString() ?? '')
  const [loiGia, setLoiGia] = useState<string | null>(null)

  useEffect(() => {
    setGiaTu(giaTri.giaTu?.toString() ?? '')
    setGiaDen(giaTri.giaDen?.toString() ?? '')
  }, [giaTri.giaTu, giaTri.giaDen])

  const chonMot = (khoa: KhoaLoc, ma: number) => onDoi({ [khoa]: giaTri[khoa] === ma ? undefined : ma })

  const apDungGia = (e: FormEvent) => {
    e.preventDefault()
    const tu = giaTu === '' ? undefined : Number(giaTu)
    const den = giaDen === '' ? undefined : Number(giaDen)
    if ((tu !== undefined && (Number.isNaN(tu) || tu < 0)) || (den !== undefined && (Number.isNaN(den) || den < 0))) {
      setLoiGia('Giá phải là số không âm.')
      return
    }
    if (tu !== undefined && den !== undefined && tu > den) {
      setLoiGia('Giá từ phải nhỏ hơn hoặc bằng giá đến.')
      return
    }
    setLoiGia(null)
    onDoi({ giaTu: tu, giaDen: den })
  }

  const danhMucGoc = danhMuc.filter((d) => d.maDanhMucCha === null)
  const coBoLoc = Object.values(giaTri).some((v) => v !== undefined)

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h2 className="font-semibold text-gray-900">Bộ lọc</h2>
        {coBoLoc && (
          <button type="button" onClick={onXoaTatCa} className="text-sm text-brand-600 hover:underline">
            Xóa tất cả
          </button>
        )}
      </div>

      <Nhom tieuDe="Loại da">
        {loaiDa.map((l) => (
          <LuaChon key={l.maLoaiDa} chon={giaTri.maLoaiDa === l.maLoaiDa} onClick={() => chonMot('maLoaiDa', l.maLoaiDa)}>
            {l.ten}
          </LuaChon>
        ))}
      </Nhom>

      <Nhom tieuDe="Danh mục">
        {danhMucGoc.map((goc) => (
          <div key={goc.maDanhMuc}>
            <LuaChon chon={giaTri.maDanhMuc === goc.maDanhMuc} onClick={() => chonMot('maDanhMuc', goc.maDanhMuc)}>
              {goc.ten}
            </LuaChon>
            {danhMuc
              .filter((c) => c.maDanhMucCha === goc.maDanhMuc)
              .map((con) => (
                <LuaChon
                  key={con.maDanhMuc}
                  thutLe
                  chon={giaTri.maDanhMuc === con.maDanhMuc}
                  onClick={() => chonMot('maDanhMuc', con.maDanhMuc)}
                >
                  {con.ten}
                </LuaChon>
              ))}
          </div>
        ))}
      </Nhom>

      <Nhom tieuDe="Thương hiệu">
        {thuongHieu.map((t) => (
          <LuaChon
            key={t.maThuongHieu}
            chon={giaTri.maThuongHieu === t.maThuongHieu}
            onClick={() => chonMot('maThuongHieu', t.maThuongHieu)}
          >
            {t.ten}
            {t.quocGia && <span className="ml-auto text-xs text-gray-400">{t.quocGia}</span>}
          </LuaChon>
        ))}
      </Nhom>

      <Nhom tieuDe="Khoảng giá">
        {MUC_GIA.map((m) => (
          <LuaChon
            key={m.nhan}
            chon={giaTri.giaTu === m.tu && giaTri.giaDen === m.den}
            onClick={() =>
              giaTri.giaTu === m.tu && giaTri.giaDen === m.den
                ? onDoi({ giaTu: undefined, giaDen: undefined })
                : onDoi({ giaTu: m.tu, giaDen: m.den })
            }
          >
            {m.nhan}
          </LuaChon>
        ))}
        <form onSubmit={apDungGia} className="pt-2">
          <div className="flex items-center gap-2">
            <input
              type="number"
              inputMode="numeric"
              min={0}
              step={1000}
              placeholder="Từ"
              aria-label="Giá từ"
              value={giaTu}
              onChange={(e) => setGiaTu(e.target.value)}
              className="w-full min-w-0 rounded-lg border border-gray-300 px-2 py-1.5 text-sm focus:border-brand-500 focus:outline-none"
            />
            <span className="text-gray-400">–</span>
            <input
              type="number"
              inputMode="numeric"
              min={0}
              step={1000}
              placeholder="Đến"
              aria-label="Giá đến"
              value={giaDen}
              onChange={(e) => setGiaDen(e.target.value)}
              className="w-full min-w-0 rounded-lg border border-gray-300 px-2 py-1.5 text-sm focus:border-brand-500 focus:outline-none"
            />
          </div>
          {loiGia && <p className="mt-1 text-xs text-red-600">{loiGia}</p>}
          <button
            type="submit"
            className="mt-2 w-full rounded-lg border border-brand-500 py-1.5 text-sm font-medium text-brand-600 hover:bg-brand-50"
          >
            Áp dụng
          </button>
        </form>
      </Nhom>
    </div>
  )
}
