"""Pipeline phân tích một đánh giá đã duyệt.

Các bước (tên bảng/cột khớp database/ReviewIntelligence_SQLServer.sql):
1. Đọc dbo.DanhGia (MaDanhGia, MaSanPham, NoiDung) với TrangThai = 'DaDuyet'.
2. ABSA (PhoBERT / LLM) → UPDATE dbo.DanhGia SET CamXuc, DoTinCay, NgayCapNhat;
   ghi dbo.KetQuaKhiaCanh (MaDanhGia, MaKhiaCanh, CamXuc, CumTuDanChung, MoHinh).
3. Embedding BGE-M3 → upsert Qdrant collection (point id = DanhGia.MaVectorQdrant,
   tạo mới nếu NULL) → UPDATE dbo.DanhGia SET MaVectorQdrant.
"""

import logging

from app.db.sqlserver import get_connection

logger = logging.getLogger(__name__)


def doc_danh_gia_da_duyet(ma_danh_gia: int) -> dict | None:
    with get_connection() as conn:
        row = (
            conn.cursor()
            .execute(
                "SELECT MaDanhGia, MaSanPham, NoiDung, MaVectorQdrant "
                "FROM dbo.DanhGia WHERE MaDanhGia = ? AND TrangThai = 'DaDuyet'",
                ma_danh_gia,
            )
            .fetchone()
        )
    if row is None:
        return None
    return {
        "ma_danh_gia": row.MaDanhGia,
        "ma_san_pham": row.MaSanPham,
        "noi_dung": row.NoiDung,
        "ma_vector_qdrant": row.MaVectorQdrant,
    }


def xu_ly_phan_tich_danh_gia(ma_danh_gia: int) -> None:
    try:
        danh_gia = doc_danh_gia_da_duyet(ma_danh_gia)
        if danh_gia is None:
            logger.warning("DanhGia#%s không tồn tại hoặc chưa DaDuyet – bỏ qua.", ma_danh_gia)
            return

        logger.info("Bắt đầu phân tích DanhGia#%s (SanPham#%s).", ma_danh_gia, danh_gia["ma_san_pham"])
        # TODO bước 2: ABSA → CamXuc, DoTinCay, KetQuaKhiaCanh
        # TODO bước 3: embedding BGE-M3 → Qdrant → DanhGia.MaVectorQdrant
    except Exception:  # noqa: BLE001 – tác vụ nền không được làm sập service
        logger.exception("Phân tích DanhGia#%s thất bại.", ma_danh_gia)
