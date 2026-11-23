import { useState, type MouseEvent, type ReactNode } from 'react'
import type { CamXuc } from '@/types/danhGia'
import { MAU_CAM_XUC, MUC, NHAN_NGAN, THU_TU_CAM_XUC } from './camXucMau'

export interface DongCamXuc {
  khoa: string
  nhan: string
  dem: Record<CamXuc, number>
}

const tong = (d: DongCamXuc) => THU_TU_CAM_XUC.reduce((s, k) => s + d.dem[k], 0)
const phanTram = (x: number, t: number) => (t === 0 ? 0 : Math.round((x / t) * 100))

/** Chú giải – luôn có khi ≥ 2 chuỗi; chữ dùng màu mực, ô màu mang danh tính. */
export function ChuGiaiCamXuc() {
  return (
    <ul className="flex flex-wrap gap-4 text-xs" style={{ color: MUC.phu }} aria-label="Chú giải">
      {THU_TU_CAM_XUC.map((k) => (
        <li key={k} className="flex items-center gap-1.5">
          <span className="h-2.5 w-2.5 rounded-sm" style={{ background: MAU_CAM_XUC[k] }} aria-hidden />
          {NHAN_NGAN[k]}
        </li>
      ))}
    </ul>
  )
}

/** Tooltip đi theo con trỏ trong một vùng chứa relative. */
function useTooltip() {
  const [tip, setTip] = useState<{ x: number; y: number; noiDung: ReactNode } | null>(null)
  const hien = (e: MouseEvent<HTMLElement | SVGElement>, noiDung: ReactNode) => {
    const khung = (e.currentTarget as Element).closest('[data-viz]')!.getBoundingClientRect()
    setTip({ x: e.clientX - khung.left, y: e.clientY - khung.top, noiDung })
  }
  const an = () => setTip(null)
  const phanTu = tip && (
    <div
      role="tooltip"
      className="pointer-events-none absolute z-10 min-w-36 -translate-x-1/2 -translate-y-[calc(100%+12px)] rounded-lg border bg-white px-3 py-2 text-xs shadow-lg"
      style={{ left: tip.x, top: tip.y, borderColor: 'rgba(11,11,11,0.10)', color: MUC.chinh }}
    >
      {tip.noiDung}
    </div>
  )
  return { hien, an, phanTu }
}

function NoiDungTooltip({ tieuDe, dem, nhan }: { tieuDe: string; dem: Record<CamXuc, number>; nhan?: CamXuc }) {
  const t = THU_TU_CAM_XUC.reduce((s, k) => s + dem[k], 0)
  return (
    <>
      <p className="mb-1 font-semibold">{tieuDe}</p>
      {THU_TU_CAM_XUC.map((k) => (
        <p key={k} className={`flex items-center justify-between gap-4 ${nhan === k ? 'font-semibold' : ''}`}>
          <span className="flex items-center gap-1.5" style={{ color: MUC.phu }}>
            <span className="h-2 w-2 rounded-sm" style={{ background: MAU_CAM_XUC[k] }} />
            {NHAN_NGAN[k]}
          </span>
          <span className="tabular-nums">
            {dem[k]} <span style={{ color: MUC.mo }}>({phanTram(dem[k], t)}%)</span>
          </span>
        </p>
      ))}
    </>
  )
}

/**
 * Thanh ngang 100% xếp chồng theo khía cạnh (tỉ lệ ý kiến tích cực / trung tính / tiêu cực).
 * Khe 2px màu nền giữa các đoạn, đầu thanh bo 4px, nhãn số trong đoạn khi đủ rộng.
 */
export function ThanhKhiaCanh({ dong }: { dong: DongCamXuc[] }) {
  const { hien, an, phanTu } = useTooltip()
  if (dong.length === 0) return <p className="py-8 text-center text-sm" style={{ color: MUC.mo }}>Chưa có dữ liệu.</p>

  return (
    <div data-viz className="relative">
      <ul className="space-y-3">
        {dong.map((d) => {
          const t = tong(d)
          return (
            <li key={d.khoa} className="grid grid-cols-[7rem_1fr_3rem] items-center gap-3 text-sm">
              <span className="truncate" style={{ color: MUC.phu }}>
                {d.nhan}
              </span>
              <div className="flex h-6 gap-[2px] overflow-hidden rounded" style={{ background: MUC.nen }}>
                {THU_TU_CAM_XUC.filter((k) => d.dem[k] > 0).map((k) => {
                  const pt = phanTram(d.dem[k], t)
                  return (
                    <div
                      key={k}
                      className="flex min-w-1 items-center justify-center text-[11px] font-medium text-white"
                      style={{ width: `${(d.dem[k] / t) * 100}%`, background: MAU_CAM_XUC[k] }}
                      onMouseMove={(e) => hien(e, <NoiDungTooltip tieuDe={d.nhan} dem={d.dem} nhan={k} />)}
                      onMouseLeave={an}
                      aria-label={`${d.nhan}: ${NHAN_NGAN[k]} ${d.dem[k]} (${pt}%)`}
                    >
                      {pt >= 14 ? d.dem[k] : ''}
                    </div>
                  )
                })}
              </div>
              <span className="text-right text-xs tabular-nums" style={{ color: MUC.mo }}>
                {t} ý
              </span>
            </li>
          )
        })}
      </ul>
      {phanTu}
    </div>
  )
}

/** Cột xếp chồng theo ngày (số đánh giá đã duyệt theo cảm xúc) – trục y một thang, lưới mờ. */
export function CotXuHuong({ dong, cao = 220 }: { dong: DongCamXuc[]; cao?: number }) {
  const { hien, an, phanTu } = useTooltip()
  if (dong.length === 0)
    return <p className="py-8 text-center text-sm" style={{ color: MUC.mo }}>Chưa có đánh giá đã phân tích trong khoảng thời gian này.</p>

  const lonNhat = Math.max(1, ...dong.map(tong))
  const buoc = lonNhat <= 5 ? 1 : Math.ceil(lonNhat / 4)
  const dinh = Math.ceil(lonNhat / buoc) * buoc
  const vach = Array.from({ length: dinh / buoc + 1 }, (_, i) => i * buoc)
  const pad = { trai: 28, duoi: 22, tren: 8 }
  const rongCot = Math.max(10, Math.min(36, 560 / dong.length - 6))
  const rong = pad.trai + dong.length * (rongCot + 6) + 6
  const y = (v: number) => pad.tren + (cao - pad.tren - pad.duoi) * (1 - v / dinh)
  const nhanMoi = Math.ceil(dong.length / 10) // tối đa ~10 nhãn ngày

  return (
    <div data-viz className="relative overflow-x-auto">
      <svg width={rong} height={cao} role="img" aria-label="Xu hướng cảm xúc theo ngày" className="block">
        {vach.map((v) => (
          <g key={v}>
            <line x1={pad.trai} x2={rong} y1={y(v)} y2={y(v)} stroke={v === 0 ? MUC.truc : MUC.luoi} strokeWidth={1} />
            <text x={pad.trai - 6} y={y(v) + 3} textAnchor="end" fontSize={10} fill={MUC.mo}>
              {v}
            </text>
          </g>
        ))}
        {dong.map((d, i) => {
          const x = pad.trai + 6 + i * (rongCot + 6)
          let day = 0
          const doan = THU_TU_CAM_XUC.filter((k) => d.dem[k] > 0)
          return (
            <g key={d.khoa} onMouseMove={(e) => hien(e, <NoiDungTooltip tieuDe={d.nhan} dem={d.dem} />)} onMouseLeave={an}>
              {/* vùng bắt rê chuột rộng hơn dấu */}
              <rect x={x - 3} y={pad.tren} width={rongCot + 6} height={cao - pad.tren - pad.duoi} fill="transparent" />
              {doan.map((k, j) => {
                const tren = y(day + d.dem[k])
                const duoi = y(day)
                day += d.dem[k]
                const laDinh = j === doan.length - 1
                const h = Math.max(1, duoi - tren - (j > 0 ? 2 : 0)) // khe 2px giữa các đoạn
                return laDinh ? (
                  <path
                    key={k}
                    d={`M${x},${tren + h} V${tren + 4} Q${x},${tren} ${x + 4},${tren} H${x + rongCot - 4} Q${x + rongCot},${tren} ${x + rongCot},${tren + 4} V${tren + h} Z`}
                    fill={MAU_CAM_XUC[k]}
                  />
                ) : (
                  <rect key={k} x={x} y={tren} width={rongCot} height={h} fill={MAU_CAM_XUC[k]} />
                )
              })}
              {i % nhanMoi === 0 && (
                <text x={x + rongCot / 2} y={cao - 6} textAnchor="middle" fontSize={10} fill={MUC.mo}>
                  {d.nhan}
                </text>
              )}
            </g>
          )
        })}
      </svg>
      {phanTu}
    </div>
  )
}

/** Bảng dữ liệu – phương án thay thế bắt buộc khi có màu tương phản thấp. */
export function BangCamXuc({ dong, cotDau }: { dong: DongCamXuc[]; cotDau: string }) {
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="border-b text-left text-xs" style={{ color: MUC.mo, borderColor: MUC.luoi }}>
          <th className="py-2 font-medium">{cotDau}</th>
          {THU_TU_CAM_XUC.map((k) => (
            <th key={k} className="py-2 text-right font-medium">
              {NHAN_NGAN[k]}
            </th>
          ))}
          <th className="py-2 text-right font-medium">Tổng</th>
        </tr>
      </thead>
      <tbody>
        {dong.map((d) => (
          <tr key={d.khoa} className="border-b last:border-0" style={{ borderColor: MUC.luoi }}>
            <td className="py-2" style={{ color: MUC.chinh }}>
              {d.nhan}
            </td>
            {THU_TU_CAM_XUC.map((k) => (
              <td key={k} className="py-2 text-right tabular-nums" style={{ color: MUC.phu }}>
                {d.dem[k]}
              </td>
            ))}
            <td className="py-2 text-right font-medium tabular-nums">{tong(d)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
