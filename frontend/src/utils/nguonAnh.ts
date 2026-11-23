/**
 * Ghi nguồn cho ảnh có giấy phép mở (CC BY-SA yêu cầu ghi công khi hiển thị).
 * Danh sách đầy đủ: frontend/public/images/san-pham/NGUON_ANH.md
 */
const OBF = { tacGia: 'Open Beauty Facts', giayPhep: 'CC BY-SA 3.0', urlGiayPhep: 'https://creativecommons.org/licenses/by-sa/3.0/deed.vi' }

const NGUON_ANH: Record<string, typeof OBF & { url: string }> = {
  '/images/san-pham/sua-rua-mat-senka-perfect-whip-anh-chup.webp': {
    ...OBF,
    url: 'https://world.openbeautyfacts.org/product/4550516474568',
  },
  '/images/san-pham/kem-duong-phuc-hoi-b5-anh-chup.webp': {
    ...OBF,
    url: 'https://world.openbeautyfacts.org/product/3337875816847',
  },
}

export const layNguonAnh = (duongDan: string | null | undefined) => (duongDan ? NGUON_ANH[duongDan] : undefined)
