"""Pipeline phân tích một đánh giá đã duyệt (tên bảng/cột khớp database/ReviewIntelligence_SQLServer.sql).

phan_tich_danh_gia:
  1. Đọc dbo.DanhGia (TrangThai = 'DaDuyet') + dbo.KhiaCanh.
  2. ABSA (LLM few-shot nếu có khóa Claude, ngược lại PhoBERT) → cảm xúc tổng + khía cạnh + cụm từ dẫn chứng.
  3. Vector nhúng (BGE-M3) → upsert Qdrant, point id = DanhGia.MaVectorQdrant (tạo mới nếu NULL).
  4. Ghi dbo.KetQuaKhiaCanh, cập nhật DanhGia.CamXuc / DoTinCay / MaVectorQdrant (1 transaction).
  5. Tính lại dbo.TomTatKhiaCanh của sản phẩm.
go_bo_danh_gia (khi DanhGia chuyển sang DaAn): xóa vector Qdrant, MaVectorQdrant = NULL, tính lại tóm tắt.
"""

import logging
import time
import uuid

from app.db import qdrant
from app.db.sqlserver import get_connection, truy_van, truy_van_mot
from app.services.absa import doc_khia_canh, lay_bo_phan_tich, mo_hinh_phan_tich
from app.services.embedding import get_bo_nhung
from app.services.tom_tat import cap_nhat_tom_tat

logger = logging.getLogger(__name__)


def phan_tich_danh_gia(ma_danh_gia: int, mo_hinh: str | None = None) -> dict:
    t0 = time.perf_counter()
    dg = truy_van_mot(
        "SELECT MaDanhGia, MaSanPham, NoiDung, SoSao, MaVectorQdrant FROM dbo.DanhGia "
        "WHERE MaDanhGia = ? AND TrangThai = 'DaDuyet'",
        ma_danh_gia,
    )
    if dg is None:
        logger.warning("DanhGia#%s không tồn tại hoặc chưa DaDuyet – bỏ qua.", ma_danh_gia)
        return {"ma_danh_gia": ma_danh_gia, "trang_thai": "bo_qua"}

    khia_canh = doc_khia_canh()
    ma_theo_ten = {k.ten: k.ma for k in khia_canh}
    bo = lay_bo_phan_tich(mo_hinh or mo_hinh_phan_tich())
    kq = bo.phan_tich(dg["NoiDung"], khia_canh)

    # ----- Qdrant -----
    nhung = get_bo_nhung()
    ten_col = qdrant.ten_collection(nhung.ten)
    qdrant.dam_bao_collection(ten_col, nhung.so_chieu)
    point_id = str(dg["MaVectorQdrant"] or uuid.uuid4())
    qdrant.upsert(ten_col, point_id, nhung.nhung([dg["NoiDung"]])[0], {
        "ma_danh_gia": dg["MaDanhGia"],
        "ma_san_pham": dg["MaSanPham"],
        "so_sao": int(dg["SoSao"]),
        "cam_xuc": kq.cam_xuc,
        "noi_dung": dg["NoiDung"],
    })

    # ----- SQL Server (1 transaction) -----
    with get_connection() as conn:
        cur = conn.cursor()
        cur.execute("DELETE FROM dbo.KetQuaKhiaCanh WHERE MaDanhGia = ?", ma_danh_gia)
        for kc in kq.khia_canh:
            if kc.ten in ma_theo_ten:
                cur.execute(
                    "INSERT dbo.KetQuaKhiaCanh (MaDanhGia, MaKhiaCanh, CamXuc, CumTuDanChung, MoHinh) "
                    "VALUES (?, ?, ?, ?, ?)",
                    ma_danh_gia, ma_theo_ten[kc.ten], kc.cam_xuc, kc.cum_tu, bo.ten,
                )
        cur.execute(
            "UPDATE dbo.DanhGia SET CamXuc = ?, DoTinCay = ?, MaVectorQdrant = ? "
            "WHERE MaDanhGia = ? AND TrangThai = 'DaDuyet'",
            kq.cam_xuc, round(kq.do_tin_cay, 4), point_id, ma_danh_gia,
        )
        con_duyet = cur.rowcount == 1
        conn.commit()

    if not con_duyet:
        # Đánh giá vừa bị ẩn trong lúc phân tích → gỡ vector để Qdrant không chứa đánh giá ẩn.
        qdrant.xoa(ten_col, point_id)

    cap_nhat_tom_tat(dg["MaSanPham"])
    ms = int((time.perf_counter() - t0) * 1000)
    logger.info("Phân tích DanhGia#%s bằng %s: %s, %s khía cạnh (%s ms).",
                ma_danh_gia, bo.ten, kq.cam_xuc, len(kq.khia_canh), ms)
    return {
        "ma_danh_gia": ma_danh_gia,
        "trang_thai": "xong",
        "mo_hinh": bo.ten,
        "cam_xuc": kq.cam_xuc,
        "do_tin_cay": round(kq.do_tin_cay, 4),
        "khia_canh": [{"ten": k.ten, "cam_xuc": k.cam_xuc, "cum_tu": k.cum_tu} for k in kq.khia_canh],
        "embedding": nhung.ten,
        "do_tre_ms": ms,
    }


def go_bo_danh_gia(ma_danh_gia: int) -> dict:
    dg = truy_van_mot("SELECT MaSanPham, MaVectorQdrant, TrangThai FROM dbo.DanhGia WHERE MaDanhGia = ?", ma_danh_gia)
    if dg is None:
        return {"ma_danh_gia": ma_danh_gia, "trang_thai": "khong_ton_tai"}
    if dg["TrangThai"] == "DaDuyet":
        return {"ma_danh_gia": ma_danh_gia, "trang_thai": "bo_qua", "ly_do": "Đánh giá vẫn đang DaDuyet."}

    if dg["MaVectorQdrant"]:
        ten_col = qdrant.ten_collection(get_bo_nhung().ten)
        qdrant.xoa(ten_col, str(dg["MaVectorQdrant"]))
        with get_connection() as conn:
            conn.cursor().execute(
                "UPDATE dbo.DanhGia SET MaVectorQdrant = NULL WHERE MaDanhGia = ? AND TrangThai <> 'DaDuyet'",
                ma_danh_gia)
            conn.commit()
    cap_nhat_tom_tat(dg["MaSanPham"])
    logger.info("Gỡ DanhGia#%s khỏi Qdrant và cập nhật tóm tắt.", ma_danh_gia)
    return {"ma_danh_gia": ma_danh_gia, "trang_thai": "xong"}


def danh_sach_can_dong_bo(tat_ca: bool = False) -> list[int]:
    """
    Đánh giá DaDuyet cần (phân tích lại): chưa có CamXuc, chưa có MaVectorQdrant, hoặc MaVectorQdrant
    không tồn tại trong collection hiện tại (vd dữ liệu mẫu, đổi embedding, ai-service tắt lúc duyệt).
    tat_ca=True: phân tích lại toàn bộ đánh giá DaDuyet.
    """
    rows = truy_van("SELECT MaDanhGia, CamXuc, MaVectorQdrant FROM dbo.DanhGia "
                    "WHERE TrangThai = 'DaDuyet' ORDER BY MaDanhGia")
    if tat_ca:
        return [r["MaDanhGia"] for r in rows]
    ten_col = qdrant.ten_collection(get_bo_nhung().ten)
    co_san = qdrant.ton_tai(ten_col, [str(r["MaVectorQdrant"]) for r in rows if r["MaVectorQdrant"]])
    return [r["MaDanhGia"] for r in rows
            if r["CamXuc"] is None or r["MaVectorQdrant"] is None or str(r["MaVectorQdrant"]) not in co_san]
