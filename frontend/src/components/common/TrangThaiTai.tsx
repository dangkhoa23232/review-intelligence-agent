export function DangTai({ nhan = 'Đang tải…' }: { nhan?: string }) {
  return (
    <div className="flex items-center justify-center gap-3 py-16 text-sm text-gray-500" role="status">
      <span className="h-5 w-5 animate-spin rounded-full border-2 border-brand-500 border-t-transparent" />
      {nhan}
    </div>
  )
}

export function HopLoi({ thongBao, onThuLai }: { thongBao: string; onThuLai?: () => void }) {
  return (
    <div className="rounded-xl border border-red-100 bg-red-50 px-4 py-6 text-center text-sm text-red-700" role="alert">
      <p>{thongBao}</p>
      {onThuLai && (
        <button type="button" onClick={onThuLai} className="mt-3 font-medium underline underline-offset-2">
          Thử lại
        </button>
      )}
    </div>
  )
}
