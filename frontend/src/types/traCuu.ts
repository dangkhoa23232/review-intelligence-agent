/** dbo.DanhMuc */
export interface DanhMuc {
  maDanhMuc: number
  maDanhMucCha: number | null
  ten: string
  duongDan: string
}

/** dbo.ThuongHieu */
export interface ThuongHieu {
  maThuongHieu: number
  ten: string
  quocGia: string | null
}

/** dbo.LoaiDa */
export interface LoaiDa {
  maLoaiDa: number
  ten: string
}

/** dbo.KhiaCanh */
export interface KhiaCanh {
  maKhiaCanh: number
  ten: string
  moTa: string | null
}
