import type { CamXuc } from '@/types/danhGia'

/**
 * Bảng màu PHÂN KỲ cho cực cảm xúc (dataviz: diverging = hai cực + điểm giữa xám trung tính).
 * Đã kiểm bằng validate_palette.js trên nền sáng #fcfcfb:
 *   cực xanh #2a78d6 ↔ đỏ #e34948: CVD ΔE 21.6, mắt thường ΔE 32.3, tương phản ≥ 3:1 – PASS.
 *   xám #a8a79f: tương phản 2.35:1 (WARN) → luôn kèm nhãn số + bảng dữ liệu.
 */
export const MAU_CAM_XUC: Record<CamXuc, string> = {
  TichCuc: '#2a78d6',
  TrungTinh: '#a8a79f',
  TieuCuc: '#e34948',
}

/** Thứ tự cố định trên trục phân kỳ: tích cực → trung tính → tiêu cực. */
export const THU_TU_CAM_XUC: CamXuc[] = ['TichCuc', 'TrungTinh', 'TieuCuc']

export const NHAN_NGAN: Record<CamXuc, string> = {
  TichCuc: 'Tích cực',
  TrungTinh: 'Trung tính',
  TieuCuc: 'Tiêu cực',
}

/** Ink & chrome (tham chiếu dataviz palette – nền sáng). */
export const MUC = {
  chinh: '#0b0b0b',
  phu: '#52514e',
  mo: '#898781',
  luoi: '#e1e0d9',
  truc: '#c3c2b7',
  nen: '#fcfcfb',
}
