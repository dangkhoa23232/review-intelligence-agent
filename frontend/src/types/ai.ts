import type { KetQuaPhanTrang } from './common'
import type { CamXuc } from './danhGia'

export type MoHinhAbsa = 'PhoBERT' | 'LLM_KhongVD' | 'LLM_ItVD'
/** CK_NhatKyTacTu_LoaiBuoc */
export type LoaiBuocTacTu = 'LapKeHoach' | 'GoiCongCu' | 'QuanSat' | 'KiemSoat' | 'TraLoi' | 'Loi'

export interface DanChung {
  maDanhGia: number
  doanTrich: string
  diem: number
  soSao: number
  daMuaHang: boolean
  tenNguoiDanhGia: string
  tenSanPham: string
}

export interface BuocTacTu {
  buocSo: number
  loaiBuoc: LoaiBuocTacTu
  tenCongCu: string | null
  dauVao: unknown
  dauRa: unknown
  doTreMs: number | null
}

export interface HoiDapResponse {
  maPhien: string
  maTinNhanHoi: number
  maTinNhan: number
  traLoi: string
  cheDo: 'tra_loi' | 'thieu_dan_chung' | 'tu_choi' | 'loi'
  sinhBang: 'llm' | 'mau'
  danChung: DanChung[]
  buoc: BuocTacTu[]
  doTreMs: number
}

export interface TraLoiTomTat {
  maTinNhan: number
  maPhien: string
  cauHoi: string | null
  traLoi: string
  maSanPham: number | null
  tenSanPham: string | null
  tenNguoiDung: string | null
  soBuoc: number
  coLoi: boolean
  soDanChung: number
  doTreMs: number | null
  ngayTao: string
}

export interface TraceChiTiet {
  tinNhan: TraLoiTomTat
  danChung: { maDanhGia: number; doanTrich: string; diem: number }[] | null
  buoc: BuocTacTu[]
}

export interface ThongKeKhiaCanh {
  maKhiaCanh: number
  tenKhiaCanh: string
  soTichCuc: number
  soTrungTinh: number
  soTieuCuc: number
}

export interface XuHuongCamXuc {
  ngay: string
  soTichCuc: number
  soTrungTinh: number
  soTieuCuc: number
}

export interface BangDieuKhien {
  maSanPham: number | null
  soNgay: number
  khiaCanh: ThongKeKhiaCanh[]
  xuHuong: XuHuongCamXuc[]
}

export interface ChiSoMoHinh {
  moHinh: MoHinhAbsa
  soMau: number
  doChinhXac: number
  doChuanXacMacro: number
  doPhuMacro: number
  diemF1Macro: number
  doTreTrungBinhMs: number | null
  khiaCanh: { doChuanXac: number; doPhu: number; diemF1: number; doChinhXacCamXuc: number; soCapChuan: number }
  ngayChayGanNhat: string | null
}

export interface ChiSoThucNghiem {
  boMau: { soKiemThu: number; soViDuMau: number; phanBoNhan: Partial<Record<CamXuc, number>> }
  moHinh: ChiSoMoHinh[]
}

export interface KetQuaMau {
  maMau: number
  noiDung: string
  camXucChuan: CamXuc
  camXucDuDoan: CamXuc
  khiaCanhChuan: Record<string, CamXuc>
  khiaCanhDuDoan: Record<string, CamXuc>
  laDung: boolean
  nhomLoi: string | null
  doTreMs: number | null
  ngayChay: string
}

/** Trạng thái mô hình từ ai-service (chuyển tiếp nguyên văn – snake_case). */
export interface TrangThaiAi {
  mo_hinh: Record<MoHinhAbsa, { san_sang: boolean; ly_do: string | null }>
  embedding: string | null
  embedding_du_phong: string | null
}

export interface TrangThaiThucNghiem {
  dang_chay: boolean
  mo_hinh?: MoHinhAbsa[]
  tien_do: Partial<Record<MoHinhAbsa, { da_xong: number; tong: number }>>
  loi: Partial<Record<MoHinhAbsa, string>>
  bo_qua?: Partial<Record<MoHinhAbsa, string>>
  bat_dau: string | null
  ket_thuc: string | null
}

export type DanhSachTrace = KetQuaPhanTrang<TraLoiTomTat>
