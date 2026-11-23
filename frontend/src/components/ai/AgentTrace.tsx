import type { BuocTacTu, LoaiBuocTacTu } from '@/types/ai'

const NHAN_BUOC: Record<LoaiBuocTacTu, string> = {
  LapKeHoach: 'Lập kế hoạch',
  GoiCongCu: 'Gọi công cụ',
  QuanSat: 'Quan sát',
  KiemSoat: 'Kiểm soát dẫn chứng',
  TraLoi: 'Trả lời',
  Loi: 'Lỗi',
}

const MAU_BUOC: Record<LoaiBuocTacTu, string> = {
  LapKeHoach: 'bg-indigo-50 text-indigo-700',
  GoiCongCu: 'bg-sky-50 text-sky-700',
  QuanSat: 'bg-gray-100 text-gray-700',
  KiemSoat: 'bg-amber-50 text-amber-800',
  TraLoi: 'bg-green-50 text-green-700',
  Loi: 'bg-red-50 text-red-700',
}

function KhoiJson({ nhan, giaTri }: { nhan: string; giaTri: unknown }) {
  if (giaTri === null || giaTri === undefined) return null
  return (
    <div className="min-w-0">
      <p className="text-[11px] font-medium uppercase tracking-wide text-gray-400">{nhan}</p>
      <pre className="mt-1 max-h-48 overflow-auto rounded-md bg-gray-50 p-2 text-[11px] leading-relaxed text-gray-700">
        {JSON.stringify(giaTri, null, 2)}
      </pre>
    </div>
  )
}

/** Dòng thời gian các bước tác tử (dbo.NhatKyTacTu). */
export default function AgentTrace({ buoc, gonGang = false }: { buoc: BuocTacTu[]; gonGang?: boolean }) {
  return (
    <ol className="space-y-3">
      {buoc.map((b) => (
        <li key={b.buocSo} className="relative border-l-2 border-gray-100 pl-4">
          <span className="absolute -left-[7px] top-1.5 h-3 w-3 rounded-full border-2 border-white bg-gray-300" aria-hidden />
          <div className="flex flex-wrap items-center gap-2 text-xs">
            <span className="font-semibold text-gray-500">#{b.buocSo}</span>
            <span className={`rounded-full px-2 py-0.5 font-medium ${MAU_BUOC[b.loaiBuoc]}`}>{NHAN_BUOC[b.loaiBuoc]}</span>
            {b.tenCongCu && <code className="rounded bg-gray-100 px-1.5 py-0.5 text-[11px] text-gray-700">{b.tenCongCu}</code>}
            {b.doTreMs !== null && <span className="text-gray-400">{b.doTreMs} ms</span>}
          </div>
          {!gonGang && (
            <div className="mt-2 grid gap-2 sm:grid-cols-2">
              <KhoiJson nhan="Đầu vào" giaTri={b.dauVao} />
              <KhoiJson nhan="Đầu ra" giaTri={b.dauRa} />
            </div>
          )}
        </li>
      ))}
    </ol>
  )
}
