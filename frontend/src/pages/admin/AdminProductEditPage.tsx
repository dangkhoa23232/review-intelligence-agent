import { useEffect, useRef, useState, type FormEvent, type ReactNode } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { DangTai, HopLoi } from '@/components/common/TrangThaiTai'
import ProductImage from '@/components/product/ProductImage'
import { adminSanPhamApi } from '@/services/api/sanPhamApi'
import { traCuuApi } from '@/services/api/traCuuApi'
import type {
  AnhTrongThuMuc,
  BienTheQuanTri,
  LuuBienTheRequest,
  LuuSanPhamRequest,
  SanPhamQuanTriChiTiet,
  TrangThaiSanPham,
} from '@/types/sanPham'
import type { DanhMuc, LoaiDa, ThuongHieu } from '@/types/traCuu'
import { dinhDangNgay, dinhDangNgayGio, dinhDangTien } from '@/utils/format'
import { thongBaoLoi } from '@/utils/useAsync'

// ============================================================================ Tiện ích

type ThongBao = { loai: 'ok' | 'loi'; noiDung: string } | null

const O_NHAP =
  'w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand-500 focus:outline-none focus:ring-1 focus:ring-brand-500'

/** ISO UTC → giá trị cho <input type="datetime-local"> theo giờ máy. */
function isoSangLocal(iso: string | null): string {
  if (!iso) return ''
  const d = new Date(iso)
  const p = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`
}

/** datetime-local (giờ máy) → ISO UTC. */
const localSangIso = (v: string): string | null => (v ? new Date(v).toISOString() : null)

function Truong({ nhan, ghiChu, children }: { nhan: string; ghiChu?: string; children: ReactNode }) {
  return (
    <label className="block">
      <span className="text-sm font-medium text-gray-800">{nhan}</span>
      <div className="mt-1">{children}</div>
      {ghiChu && <span className="mt-1 block text-xs text-gray-500">{ghiChu}</span>}
    </label>
  )
}

function DongThongBao({ tb }: { tb: ThongBao }) {
  if (!tb) return null
  return (
    <p
      role="status"
      className={`rounded-lg px-3 py-2 text-sm ${tb.loai === 'ok' ? 'bg-green-50 text-green-800' : 'bg-red-50 text-red-700'}`}
    >
      {tb.noiDung}
    </p>
  )
}

function The({ tieuDe, moTa, children }: { tieuDe: string; moTa?: string; children: ReactNode }) {
  return (
    <section className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm sm:p-6">
      <h2 className="text-lg font-semibold text-gray-900">{tieuDe}</h2>
      {moTa && <p className="mt-0.5 text-sm text-gray-500">{moTa}</p>}
      <div className="mt-4">{children}</div>
    </section>
  )
}

// ============================================================================ Ảnh

/** Lưới ảnh có sẵn trong frontend/public/images/san-pham (GET /api/admin/anh-san-pham) – bấm để thêm. */
function ChonAnhThuMuc({ daChon, onChon, onDong }: { daChon: string[]; onChon: (a: string) => void; onDong: () => void }) {
  const [ds, setDs] = useState<AnhTrongThuMuc[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [loc, setLoc] = useState('')

  useEffect(() => {
    const ctrl = new AbortController()
    adminSanPhamApi
      .anhTrongThuMuc(ctrl.signal)
      .then((x) => !ctrl.signal.aborted && setDs(x))
      .catch((err) => !ctrl.signal.aborted && setLoi(thongBaoLoi(err)))
    return () => ctrl.abort()
  }, [])

  const hienThi = (ds ?? []).filter((a) => a.tenTep.toLowerCase().includes(loc.trim().toLowerCase()))

  return (
    <div className="rounded-xl border border-gray-200 bg-gray-50 p-3">
      <div className="flex flex-wrap items-center gap-2">
        <input
          value={loc}
          onChange={(e) => setLoc(e.target.value)}
          placeholder="Lọc theo tên tệp…"
          className={`${O_NHAP} max-w-xs bg-white`}
        />
        <span className="text-xs text-gray-500">
          {ds ? `${hienThi.length}/${ds.length} ảnh trong /images/san-pham` : ''}
        </span>
        <button type="button" onClick={onDong} className="ml-auto rounded-lg border border-gray-300 bg-white px-3 py-1.5 text-sm hover:bg-gray-100">
          Đóng
        </button>
      </div>
      {loi ? (
        <p className="mt-3 text-sm text-red-600">{loi}</p>
      ) : !ds ? (
        <DangTai nhan="Đang tải danh sách ảnh…" />
      ) : (
        <ul className="mt-3 grid max-h-80 grid-cols-3 gap-2 overflow-y-auto sm:grid-cols-6">
          {hienThi.map((a) => {
            const co = daChon.includes(a.duongDan)
            return (
              <li key={a.tenTep}>
                <button
                  type="button"
                  disabled={co}
                  onClick={() => onChon(a.duongDan)}
                  title={a.tenTep}
                  className={`block w-full overflow-hidden rounded-lg border bg-white text-left ${co ? 'border-brand-500 opacity-50' : 'border-gray-200 hover:border-brand-400'}`}
                >
                  <ProductImage src={a.duongDan} alt={a.tenTep} className="aspect-square w-full p-1 text-base" />
                  <span className="block truncate px-1 pb-1 text-[10px] text-gray-600">{co ? '✓ Đã chọn' : a.tenTep}</span>
                </button>
              </li>
            )
          })}
        </ul>
      )}
    </div>
  )
}

function QuanLyAnh({ anh, onChange }: { anh: string[]; onChange: (a: string[]) => void }) {
  const [moThuMuc, setMoThuMuc] = useState(false)
  const [dangTai, setDangTai] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const [url, setUrl] = useState('')
  const inputRef = useRef<HTMLInputElement>(null)

  async function chonTep(files: FileList | null) {
    if (!files || files.length === 0) return
    setDangTai(true)
    setLoi(null)
    const moi: string[] = []
    try {
      for (const tep of Array.from(files)) {
        if (anh.length + moi.length >= 10) throw new Error('Tối đa 10 ảnh cho mỗi sản phẩm.')
        if (!/\.(jpe?g|png|webp)$/i.test(tep.name)) throw new Error(`“${tep.name}”: chỉ nhận .jpg, .png, .webp.`)
        if (tep.size > 5 * 1024 * 1024) throw new Error(`“${tep.name}” vượt quá 5 MB.`)
        moi.push((await adminSanPhamApi.taiAnh(tep)).duongDan)
      }
    } catch (err) {
      setLoi(err instanceof Error && !('status' in err) ? err.message : thongBaoLoi(err))
    } finally {
      if (moi.length > 0) onChange([...anh, ...moi])
      setDangTai(false)
      if (inputRef.current) inputRef.current.value = ''
    }
  }

  function themUrl() {
    const v = url.trim()
    if (!v) return
    if (anh.includes(v)) return setLoi('Ảnh này đã có trong danh sách.')
    if (!v.startsWith('/') && !/^https?:\/\//i.test(v)) return setLoi("Đường dẫn phải bắt đầu bằng '/' hoặc http(s)://")
    onChange([...anh, v])
    setUrl('')
    setLoi(null)
  }

  function doiCho(i: number, j: number) {
    const a = [...anh]
    ;[a[i], a[j]] = [a[j], a[i]]
    onChange(a)
  }

  return (
    <div className="space-y-3">
      {anh.length > 0 ? (
        <ul className="grid grid-cols-3 gap-3 sm:grid-cols-5">
          {anh.map((a, i) => (
            <li key={a} className="group relative overflow-hidden rounded-xl border border-gray-200">
              <ProductImage src={a} alt={`Ảnh ${i + 1}`} className="aspect-square w-full text-base" />
              {i === 0 && (
                <span className="absolute left-1.5 top-1.5 rounded-full bg-brand-500 px-2 py-0.5 text-[10px] font-semibold text-white">
                  Ảnh đại diện
                </span>
              )}
              <div className="absolute inset-x-0 bottom-0 flex justify-between bg-white/90 px-1 py-1 text-xs">
                <span className="flex gap-1">
                  <button
                    type="button"
                    disabled={i === 0}
                    onClick={() => doiCho(i, i - 1)}
                    aria-label="Đưa lên trước"
                    className="rounded px-1.5 hover:bg-gray-100 disabled:opacity-30"
                  >
                    ←
                  </button>
                  <button
                    type="button"
                    disabled={i === anh.length - 1}
                    onClick={() => doiCho(i, i + 1)}
                    aria-label="Đưa ra sau"
                    className="rounded px-1.5 hover:bg-gray-100 disabled:opacity-30"
                  >
                    →
                  </button>
                </span>
                <button
                  type="button"
                  onClick={() => onChange(anh.filter((x) => x !== a))}
                  className="rounded px-1.5 text-red-600 hover:bg-red-50"
                >
                  Xóa
                </button>
              </div>
            </li>
          ))}
        </ul>
      ) : (
        <p className="rounded-xl border border-dashed border-gray-300 px-4 py-6 text-center text-sm text-gray-500">
          Chưa có ảnh – sản phẩm sẽ hiển thị ảnh mặc định.
        </p>
      )}

      <div className="flex flex-wrap items-center gap-2">
        <input
          ref={inputRef}
          type="file"
          accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp"
          multiple
          onChange={(e) => chonTep(e.target.files)}
          className="hidden"
          id="tai-anh-san-pham"
        />
        <label
          htmlFor="tai-anh-san-pham"
          className={`cursor-pointer rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700 ${dangTai ? 'pointer-events-none opacity-60' : ''}`}
        >
          {dangTai ? 'Đang tải ảnh…' : 'Tải ảnh lên'}
        </label>
        <button
          type="button"
          onClick={() => setMoThuMuc((x) => !x)}
          className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
        >
          Chọn từ thư mục
        </button>
        <span className="text-xs text-gray-500">.jpg / .png / .webp, tối đa 5 MB mỗi ảnh</span>
      </div>
      {moThuMuc && (
        <ChonAnhThuMuc
          daChon={anh}
          onDong={() => setMoThuMuc(false)}
          onChon={(a) => {
            if (anh.length >= 10) return setLoi('Tối đa 10 ảnh cho mỗi sản phẩm.')
            onChange([...anh, a])
          }}
        />
      )}
      <div className="flex gap-2">
        <input
          value={url}
          onChange={(e) => setUrl(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && (e.preventDefault(), themUrl())}
          placeholder="Hoặc dán đường dẫn ảnh: /images/san-pham/... hoặc https://..."
          className={O_NHAP}
        />
        <button type="button" onClick={themUrl} className="shrink-0 rounded-lg border border-gray-300 px-3 text-sm hover:bg-gray-50">
          Thêm
        </button>
      </div>
      {loi && <p className="text-sm text-red-600">{loi}</p>}
    </div>
  )
}

// ============================================================================ Thông tin sản phẩm

interface DuLieuTraCuu {
  danhMuc: DanhMuc[]
  thuongHieu: ThuongHieu[]
  loaiDa: LoaiDa[]
}

function formTuSanPham(sp: SanPhamQuanTriChiTiet | null): LuuSanPhamRequest {
  return {
    maDanhMuc: sp?.maDanhMuc ?? 0,
    maThuongHieu: sp?.maThuongHieu ?? 0,
    ten: sp?.ten ?? '',
    duongDan: sp?.duongDan ?? '',
    moTa: sp?.moTa ?? '',
    congDung: sp?.congDung ?? '',
    thanhPhan: sp?.thanhPhan ?? '',
    huongDanSuDung: sp?.huongDanSuDung ?? '',
    danhSachAnh: sp?.danhSachAnh ?? [],
    maLoaiDa: sp?.loaiDa.map((l) => l.maLoaiDa) ?? [],
    trangThai: sp?.trangThai ?? 'DangBan',
  }
}

function FormThongTin({
  sp,
  traCuu,
  onDaLuu,
}: {
  sp: SanPhamQuanTriChiTiet | null
  traCuu: DuLieuTraCuu
  onDaLuu: (sp: SanPhamQuanTriChiTiet, laTaoMoi: boolean) => void
}) {
  const [f, setF] = useState<LuuSanPhamRequest>(() => formTuSanPham(sp))
  const [dangLuu, setDangLuu] = useState(false)
  const [tb, setTb] = useState<ThongBao>(null)
  // Chỉ nạp lại form khi đổi sản phẩm – thao tác biến thể/lô hàng không xóa thay đổi chưa lưu ở đây.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => setF(formTuSanPham(sp)), [sp?.maSanPham])

  const dat = <K extends keyof LuuSanPhamRequest>(k: K, v: LuuSanPhamRequest[K]) => setF((x) => ({ ...x, [k]: v }))

  // Danh mục: nhóm theo danh mục cha để chọn dễ hơn.
  const danhMucCha = traCuu.danhMuc.filter((d) => d.maDanhMucCha === null)

  async function luu(e: FormEvent) {
    e.preventDefault()
    if (!f.maDanhMuc || !f.maThuongHieu) return setTb({ loai: 'loi', noiDung: 'Vui lòng chọn danh mục và thương hiệu.' })
    setDangLuu(true)
    setTb(null)
    try {
      const body: LuuSanPhamRequest = { ...f, ten: f.ten.trim(), duongDan: f.duongDan?.trim() || undefined }
      const kq = sp ? await adminSanPhamApi.capNhat(sp.maSanPham, body) : await adminSanPhamApi.tao(body)
      setTb({ loai: 'ok', noiDung: sp ? 'Đã lưu thay đổi.' : 'Đã tạo sản phẩm. Tiếp tục thêm biến thể và nhập lô hàng.' })
      onDaLuu(kq, !sp)
    } catch (err) {
      setTb({ loai: 'loi', noiDung: thongBaoLoi(err) })
    } finally {
      setDangLuu(false)
    }
  }

  return (
    <form onSubmit={luu} className="space-y-6">
      <The tieuDe="Thông tin sản phẩm">
        <div className="grid gap-4 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <Truong nhan="Tên sản phẩm *">
              <input
                required
                minLength={3}
                maxLength={255}
                value={f.ten}
                onChange={(e) => dat('ten', e.target.value)}
                className={O_NHAP}
              />
            </Truong>
          </div>
          <Truong nhan="Danh mục *">
            <select
              required
              value={f.maDanhMuc || ''}
              onChange={(e) => dat('maDanhMuc', Number(e.target.value))}
              className={O_NHAP}
            >
              <option value="">— Chọn danh mục —</option>
              {danhMucCha.map((cha) => (
                <optgroup key={cha.maDanhMuc} label={cha.ten}>
                  <option value={cha.maDanhMuc}>{cha.ten} (chung)</option>
                  {traCuu.danhMuc
                    .filter((c) => c.maDanhMucCha === cha.maDanhMuc)
                    .map((c) => (
                      <option key={c.maDanhMuc} value={c.maDanhMuc}>
                        {c.ten}
                      </option>
                    ))}
                </optgroup>
              ))}
            </select>
          </Truong>
          <Truong nhan="Thương hiệu *">
            <select
              required
              value={f.maThuongHieu || ''}
              onChange={(e) => dat('maThuongHieu', Number(e.target.value))}
              className={O_NHAP}
            >
              <option value="">— Chọn thương hiệu —</option>
              {traCuu.thuongHieu.map((t) => (
                <option key={t.maThuongHieu} value={t.maThuongHieu}>
                  {t.ten}
                  {t.quocGia ? ` (${t.quocGia})` : ''}
                </option>
              ))}
            </select>
          </Truong>
          <Truong nhan="Đường dẫn (slug)" ghiChu={sp ? 'Bỏ trống để giữ nguyên.' : 'Bỏ trống để tự sinh từ tên.'}>
            <input
              value={f.duongDan ?? ''}
              onChange={(e) => dat('duongDan', e.target.value.toLowerCase())}
              pattern="[a-z0-9]+(-[a-z0-9]+)*"
              maxLength={280}
              placeholder="vd: sua-rua-mat-senka-perfect-whip"
              className={O_NHAP}
            />
          </Truong>
          <Truong nhan="Trạng thái">
            <select
              value={f.trangThai}
              onChange={(e) => dat('trangThai', e.target.value as TrangThaiSanPham)}
              className={O_NHAP}
            >
              <option value="DangBan">Đang bán</option>
              <option value="NgungBan">Ngừng bán</option>
            </select>
          </Truong>
          <div className="sm:col-span-2">
            <Truong nhan="Công dụng" ghiChu="Một dòng ngắn hiển thị trên thẻ sản phẩm (tối đa 300 ký tự).">
              <input maxLength={300} value={f.congDung ?? ''} onChange={(e) => dat('congDung', e.target.value)} className={O_NHAP} />
            </Truong>
          </div>
          <div className="sm:col-span-2">
            <Truong nhan="Mô tả">
              <textarea rows={4} maxLength={8000} value={f.moTa ?? ''} onChange={(e) => dat('moTa', e.target.value)} className={O_NHAP} />
            </Truong>
          </div>
          <Truong nhan="Thành phần">
            <textarea
              rows={3}
              maxLength={4000}
              value={f.thanhPhan ?? ''}
              onChange={(e) => dat('thanhPhan', e.target.value)}
              className={O_NHAP}
            />
          </Truong>
          <Truong nhan="Hướng dẫn sử dụng">
            <textarea
              rows={3}
              maxLength={1000}
              value={f.huongDanSuDung ?? ''}
              onChange={(e) => dat('huongDanSuDung', e.target.value)}
              className={O_NHAP}
            />
          </Truong>
        </div>

        <fieldset className="mt-5">
          <legend className="text-sm font-medium text-gray-800">Loại da phù hợp</legend>
          <div className="mt-2 flex flex-wrap gap-2">
            {traCuu.loaiDa.map((l) => {
              const chon = f.maLoaiDa.includes(l.maLoaiDa)
              return (
                <label
                  key={l.maLoaiDa}
                  className={`cursor-pointer rounded-full border px-3 py-1 text-sm transition-colors ${
                    chon ? 'border-brand-500 bg-brand-50 text-brand-700' : 'border-gray-300 text-gray-700 hover:border-brand-300'
                  }`}
                >
                  <input
                    type="checkbox"
                    className="sr-only"
                    checked={chon}
                    onChange={() =>
                      dat('maLoaiDa', chon ? f.maLoaiDa.filter((x) => x !== l.maLoaiDa) : [...f.maLoaiDa, l.maLoaiDa])
                    }
                  />
                  {l.ten}
                </label>
              )
            })}
          </div>
        </fieldset>
      </The>

      <The tieuDe="Ảnh sản phẩm" moTa="Ảnh đầu tiên là ảnh đại diện trên thẻ sản phẩm. Nhấn “Lưu” để ghi danh sách ảnh.">
        <QuanLyAnh anh={f.danhSachAnh} onChange={(a) => dat('danhSachAnh', a)} />
      </The>

      <div className="sticky bottom-0 z-10 -mx-4 flex flex-wrap items-center gap-3 border-t border-gray-100 bg-white/95 px-4 py-3 backdrop-blur">
        <button
          type="submit"
          disabled={dangLuu}
          className="rounded-full bg-brand-500 px-6 py-2.5 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-60"
        >
          {dangLuu ? 'Đang lưu…' : sp ? 'Lưu thay đổi' : 'Tạo sản phẩm'}
        </button>
        <div className="min-w-0 flex-1">
          <DongThongBao tb={tb} />
        </div>
      </div>
    </form>
  )
}

// ============================================================================ Biến thể

function formBienThe(bt?: BienTheQuanTri) {
  return {
    maSKU: bt?.maSKU ?? '',
    dungTich: bt?.dungTich ?? '',
    giaBan: bt ? String(bt.giaBan) : '',
    giaKhuyenMai: bt?.giaKhuyenMai != null ? String(bt.giaKhuyenMai) : '',
    khuyenMaiTu: isoSangLocal(bt?.khuyenMaiTu ?? null),
    khuyenMaiDen: isoSangLocal(bt?.khuyenMaiDen ?? null),
    dangHoatDong: bt?.dangHoatDong ?? true,
  }
}

function FormBienThe({
  bt,
  onLuu,
  onHuy,
}: {
  bt?: BienTheQuanTri
  onLuu: (req: LuuBienTheRequest) => Promise<void>
  onHuy?: () => void
}) {
  const [f, setF] = useState(() => formBienThe(bt))
  const [dangLuu, setDangLuu] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)

  async function luu(e: FormEvent) {
    e.preventDefault()
    const giaBan = Number(f.giaBan)
    const giaKm = f.giaKhuyenMai === '' ? null : Number(f.giaKhuyenMai)
    if (!Number.isFinite(giaBan) || giaBan < 0) return setLoi('Giá bán không hợp lệ.')
    if (giaKm !== null) {
      if (!(giaKm < giaBan)) return setLoi('Giá khuyến mãi phải nhỏ hơn giá bán.')
      if (!f.khuyenMaiTu || !f.khuyenMaiDen) return setLoi('Cần nhập thời gian bắt đầu và kết thúc khuyến mãi.')
      if (new Date(f.khuyenMaiDen) <= new Date(f.khuyenMaiTu)) return setLoi('Kết thúc khuyến mãi phải sau thời điểm bắt đầu.')
    }
    setDangLuu(true)
    setLoi(null)
    try {
      await onLuu({
        maSKU: f.maSKU.trim(),
        dungTich: f.dungTich.trim(),
        giaBan,
        giaKhuyenMai: giaKm,
        khuyenMaiTu: giaKm === null ? null : localSangIso(f.khuyenMaiTu),
        khuyenMaiDen: giaKm === null ? null : localSangIso(f.khuyenMaiDen),
        dangHoatDong: f.dangHoatDong,
      })
      if (!bt) setF(formBienThe())
    } catch (err) {
      setLoi(thongBaoLoi(err))
    } finally {
      setDangLuu(false)
    }
  }

  return (
    <form onSubmit={luu} className="space-y-3 rounded-xl bg-gray-50 p-4">
      <div className="grid gap-3 sm:grid-cols-4">
        <Truong nhan="Mã SKU *">
          <input
            required
            maxLength={50}
            pattern="[A-Za-z0-9][A-Za-z0-9._\-]*"
            value={f.maSKU}
            onChange={(e) => setF({ ...f, maSKU: e.target.value.toUpperCase() })}
            placeholder="SEN-WHIP-120"
            className={O_NHAP}
          />
        </Truong>
        <Truong nhan="Dung tích *">
          <input
            required
            maxLength={30}
            value={f.dungTich}
            onChange={(e) => setF({ ...f, dungTich: e.target.value })}
            placeholder="120g, 50ml…"
            className={O_NHAP}
          />
        </Truong>
        <Truong nhan="Giá bán (đ) *">
          <input
            required
            type="number"
            min={0}
            step={1000}
            value={f.giaBan}
            onChange={(e) => setF({ ...f, giaBan: e.target.value })}
            className={O_NHAP}
          />
        </Truong>
        <Truong nhan="Giá khuyến mãi (đ)">
          <input
            type="number"
            min={0}
            step={1000}
            value={f.giaKhuyenMai}
            onChange={(e) => setF({ ...f, giaKhuyenMai: e.target.value })}
            placeholder="Bỏ trống nếu không"
            className={O_NHAP}
          />
        </Truong>
        {f.giaKhuyenMai !== '' && (
          <>
            <Truong nhan="Khuyến mãi từ *">
              <input
                type="datetime-local"
                value={f.khuyenMaiTu}
                onChange={(e) => setF({ ...f, khuyenMaiTu: e.target.value })}
                className={O_NHAP}
              />
            </Truong>
            <Truong nhan="Khuyến mãi đến *">
              <input
                type="datetime-local"
                value={f.khuyenMaiDen}
                onChange={(e) => setF({ ...f, khuyenMaiDen: e.target.value })}
                className={O_NHAP}
              />
            </Truong>
          </>
        )}
        <label className="flex items-center gap-2 self-end pb-2 text-sm text-gray-700">
          <input
            type="checkbox"
            checked={f.dangHoatDong}
            onChange={(e) => setF({ ...f, dangHoatDong: e.target.checked })}
            className="h-4 w-4 accent-brand-500"
          />
          Đang hoạt động
        </label>
      </div>
      {loi && <p className="text-sm text-red-600">{loi}</p>}
      <div className="flex gap-2">
        <button
          type="submit"
          disabled={dangLuu}
          className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700 disabled:opacity-60"
        >
          {dangLuu ? 'Đang lưu…' : bt ? 'Lưu biến thể' : 'Thêm biến thể'}
        </button>
        {onHuy && (
          <button type="button" onClick={onHuy} className="rounded-lg border border-gray-300 px-4 py-2 text-sm hover:bg-white">
            Hủy
          </button>
        )}
      </div>
    </form>
  )
}

function FormNhapLo({ onNhap }: { onNhap: (soLo: string, hanSuDung: string, soLuong: number) => Promise<void> }) {
  const [soLo, setSoLo] = useState('')
  const [han, setHan] = useState('')
  const [sl, setSl] = useState('')
  const [dangLuu, setDangLuu] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const ngayMai = new Date(Date.now() + 86_400_000).toISOString().slice(0, 10)

  async function nhap(e: FormEvent) {
    e.preventDefault()
    const n = Number(sl)
    if (!Number.isInteger(n) || n < 1) return setLoi('Số lượng nhập phải là số nguyên ≥ 1.')
    setDangLuu(true)
    setLoi(null)
    try {
      await onNhap(soLo.trim(), han, n)
      setSoLo('')
      setHan('')
      setSl('')
    } catch (err) {
      setLoi(thongBaoLoi(err))
    } finally {
      setDangLuu(false)
    }
  }

  return (
    <form onSubmit={nhap} className="mt-3 flex flex-wrap items-end gap-2">
      <Truong nhan="Số lô">
        <input
          required
          maxLength={50}
          pattern="[A-Za-z0-9][A-Za-z0-9._/\-]*"
          value={soLo}
          onChange={(e) => setSoLo(e.target.value.toUpperCase())}
          placeholder="L2612"
          className={`${O_NHAP} w-32`}
        />
      </Truong>
      <Truong nhan="Hạn sử dụng">
        <input required type="date" min={ngayMai} value={han} onChange={(e) => setHan(e.target.value)} className={`${O_NHAP} w-40`} />
      </Truong>
      <Truong nhan="Số lượng">
        <input
          required
          type="number"
          min={1}
          max={100000}
          value={sl}
          onChange={(e) => setSl(e.target.value)}
          className={`${O_NHAP} w-28`}
        />
      </Truong>
      <button
        type="submit"
        disabled={dangLuu}
        className="rounded-lg bg-brand-500 px-4 py-2 text-sm font-medium text-white hover:bg-brand-600 disabled:opacity-60"
      >
        {dangLuu ? 'Đang nhập…' : 'Nhập lô'}
      </button>
      {loi && <p className="w-full text-sm text-red-600">{loi}</p>}
    </form>
  )
}

function TheBienThe({
  bt,
  maSanPham,
  onCapNhat,
}: {
  bt: BienTheQuanTri
  maSanPham: number
  onCapNhat: (sp: SanPhamQuanTriChiTiet, thongBao: string) => void
}) {
  const [dangSua, setDangSua] = useState(false)
  const now = Date.now()
  const kmHieuLuc =
    bt.giaKhuyenMai !== null &&
    bt.khuyenMaiTu !== null &&
    bt.khuyenMaiDen !== null &&
    new Date(bt.khuyenMaiTu).getTime() <= now &&
    new Date(bt.khuyenMaiDen).getTime() >= now

  return (
    <li className="rounded-xl border border-gray-200 p-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className="font-semibold text-gray-900">
            {bt.dungTich} <span className="ml-1 font-mono text-xs font-normal text-gray-500">{bt.maSKU}</span>
            {!bt.dangHoatDong && (
              <span className="ml-2 rounded-full bg-gray-100 px-2 py-0.5 text-xs font-normal text-gray-600">Ngừng hoạt động</span>
            )}
          </p>
          <p className="mt-1 text-sm">
            <span className="font-medium text-gray-900">{dinhDangTien(bt.giaBan)}</span>
            {bt.giaKhuyenMai !== null && (
              <span className={kmHieuLuc ? 'text-brand-600' : 'text-gray-400'}>
                {' '}
                · KM {dinhDangTien(bt.giaKhuyenMai)} ({dinhDangNgayGio(bt.khuyenMaiTu!)} – {dinhDangNgayGio(bt.khuyenMaiDen!)})
                {!kmHieuLuc && ' – không hiệu lực'}
              </span>
            )}
          </p>
        </div>
        <div className="flex items-center gap-3">
          <span
            className={`rounded-full px-3 py-1 text-sm font-semibold tabular-nums ${
              bt.soLuongTon === 0 ? 'bg-red-50 text-red-700' : bt.soLuongTon <= 10 ? 'bg-amber-50 text-amber-700' : 'bg-green-50 text-green-700'
            }`}
          >
            Tồn: {bt.soLuongTon}
          </span>
          {!dangSua && (
            <button
              type="button"
              onClick={() => setDangSua(true)}
              className="rounded-lg border border-gray-300 px-3 py-1 text-xs font-medium text-gray-700 hover:bg-gray-50"
            >
              Sửa
            </button>
          )}
        </div>
      </div>

      {dangSua && (
        <div className="mt-3">
          <FormBienThe
            bt={bt}
            onHuy={() => setDangSua(false)}
            onLuu={async (req) => {
              const sp = await adminSanPhamApi.capNhatBienThe(maSanPham, bt.maBienThe, req)
              setDangSua(false)
              onCapNhat(sp, `Đã cập nhật biến thể ${req.maSKU}.`)
            }}
          />
        </div>
      )}

      <div className="mt-4">
        <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">Lô hàng (xuất theo hạn gần nhất trước)</p>
        {bt.loHang.length > 0 ? (
          <div className="mt-2 overflow-x-auto">
            <table className="w-full min-w-[480px] text-sm">
              <thead className="text-left text-xs text-gray-500">
                <tr>
                  <th className="py-1 pr-3 font-medium">Số lô</th>
                  <th className="py-1 pr-3 font-medium">Hạn sử dụng</th>
                  <th className="py-1 pr-3 text-right font-medium">Nhập</th>
                  <th className="py-1 pr-3 text-right font-medium">Còn lại</th>
                  <th className="py-1 font-medium">Ngày nhập</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {bt.loHang.map((l) => (
                  <tr key={l.maLo}>
                    <td className="py-1.5 pr-3 font-mono text-xs">{l.soLo}</td>
                    <td className="py-1.5 pr-3">{dinhDangNgay(l.hanSuDung)}</td>
                    <td className="py-1.5 pr-3 text-right tabular-nums">{l.soLuongNhap}</td>
                    <td className="py-1.5 pr-3 text-right tabular-nums">{l.soLuongConLai}</td>
                    <td className="py-1.5 text-gray-500">{dinhDangNgayGio(l.ngayNhap)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="mt-1 text-sm text-gray-500">Chưa nhập lô nào – tồn kho đang là 0.</p>
        )}
        <FormNhapLo
          onNhap={async (soLo, hanSuDung, soLuongNhap) => {
            const sp = await adminSanPhamApi.nhapLoHang(maSanPham, bt.maBienThe, { soLo, hanSuDung, soLuongNhap })
            onCapNhat(sp, `Đã nhập lô ${soLo} (+${soLuongNhap}) cho ${bt.maSKU}.`)
          }}
        />
      </div>
    </li>
  )
}

function KhuBienThe({ sp, onCapNhat }: { sp: SanPhamQuanTriChiTiet; onCapNhat: (sp: SanPhamQuanTriChiTiet) => void }) {
  const [tb, setTb] = useState<ThongBao>(null)
  const capNhat = (moi: SanPhamQuanTriChiTiet, thongBao: string) => {
    onCapNhat(moi)
    setTb({ loai: 'ok', noiDung: thongBao })
  }

  return (
    <The
      tieuDe="Biến thể & tồn kho"
      moTa="Mỗi dung tích là một SKU. Tồn kho chỉ tăng khi nhập lô hàng và giảm khi khách đặt hàng."
    >
      <div className="space-y-4">
        <DongThongBao tb={tb} />
        {sp.bienThe.length > 0 ? (
          <ul className="space-y-3">
            {sp.bienThe.map((bt) => (
              <TheBienThe key={bt.maBienThe} bt={bt} maSanPham={sp.maSanPham} onCapNhat={capNhat} />
            ))}
          </ul>
        ) : (
          <p className="text-sm text-gray-500">Chưa có biến thể – khách hàng chưa thể mua sản phẩm này.</p>
        )}
        <div>
          <p className="mb-2 text-sm font-medium text-gray-800">Thêm biến thể dung tích</p>
          <FormBienThe
            onLuu={async (req) => {
              capNhat(await adminSanPhamApi.themBienThe(sp.maSanPham, req), `Đã thêm biến thể ${req.maSKU}. Hãy nhập lô hàng để có tồn kho.`)
            }}
          />
        </div>
      </div>
    </The>
  )
}

// ============================================================================ Trang

export default function AdminProductEditPage() {
  const { ma } = useParams()
  const laTaoMoi = ma === 'moi'
  const maSanPham = laTaoMoi ? null : Number(ma)
  const navigate = useNavigate()

  const [traCuu, setTraCuu] = useState<DuLieuTraCuu | null>(null)
  const [sp, setSp] = useState<SanPhamQuanTriChiTiet | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [lan, setLan] = useState(0)

  useEffect(() => {
    const ctrl = new AbortController()
    setLoi(null)
    if (maSanPham !== null && (!Number.isInteger(maSanPham) || maSanPham <= 0)) {
      setLoi('Mã sản phẩm không hợp lệ.')
      return
    }
    Promise.all([
      traCuuApi.danhMuc(),
      traCuuApi.thuongHieu(),
      traCuuApi.loaiDa(),
      maSanPham !== null ? adminSanPhamApi.chiTiet(maSanPham, ctrl.signal) : Promise.resolve(null),
    ])
      .then(([danhMuc, thuongHieu, loaiDa, chiTiet]) => {
        if (ctrl.signal.aborted) return
        setTraCuu({ danhMuc, thuongHieu, loaiDa })
        setSp(chiTiet)
      })
      .catch((err) => !ctrl.signal.aborted && setLoi(thongBaoLoi(err)))
    return () => ctrl.abort()
  }, [maSanPham, lan])

  if (loi) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16">
        <HopLoi thongBao={loi} onThuLai={() => setLan((x) => x + 1)} />
      </div>
    )
  }
  if (!traCuu || (maSanPham !== null && !sp)) return <DangTai />

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <nav className="text-sm text-gray-500" aria-label="Đường dẫn">
        <Link to="/admin/san-pham" className="hover:text-brand-600">
          Sản phẩm
        </Link>
        <span className="mx-2">/</span>
        <span className="text-gray-800">{sp ? sp.ten : 'Thêm mới'}</span>
      </nav>
      <div className="mt-2 flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold text-gray-900">{sp ? 'Sửa sản phẩm' : 'Thêm sản phẩm'}</h1>
        {sp && sp.trangThai === 'DangBan' && (
          <Link to={`/san-pham/${sp.duongDan}`} className="text-sm font-medium text-brand-600 hover:underline">
            Xem trang sản phẩm ↗
          </Link>
        )}
      </div>

      <div className="mt-6 space-y-6">
        <FormThongTin
          sp={sp}
          traCuu={traCuu}
          onDaLuu={(moi, taoMoi) => {
            setSp(moi)
            if (taoMoi) navigate(`/admin/san-pham/${moi.maSanPham}`, { replace: true })
          }}
        />
        {sp ? (
          <KhuBienThe sp={sp} onCapNhat={setSp} />
        ) : (
          <p className="rounded-2xl bg-gray-50 px-4 py-6 text-center text-sm text-gray-500">
            Tạo sản phẩm trước, sau đó thêm biến thể dung tích và nhập lô hàng.
          </p>
        )}
      </div>
    </div>
  )
}
