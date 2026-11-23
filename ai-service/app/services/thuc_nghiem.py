"""Thực nghiệm so sánh cấu hình ABSA trên dbo.MauKiemThu (TapDuLieu = 'KiemThu').

Ghi dbo.KetQuaThucNghiem (MERGE theo UQ (MaMau, MoHinh) – chạy lại sẽ thay kết quả cũ);
chỉ số tổng hợp đọc từ dbo.vw_ChiSoThucNghiem.
Chỉ ghi kết quả của mô hình THỰC SỰ chạy được – cấu hình thiếu mô hình/khóa API bị bỏ qua và báo lý do.
"""

import json
import logging
import re
import threading
import time
from datetime import datetime, timezone
from pathlib import Path

from app.core.config import THU_MUC_GOC
from app.db.sqlserver import get_connection, truy_van
from app.services.absa import doc_khia_canh, lay_bo_phan_tich
from app.services.absa.base import MO_HINH, MoHinhKhongSanSang

logger = logging.getLogger(__name__)

FILE_MAU = THU_MUC_GOC / "data" / "mau_kiem_thu.json"
_TUONG_PHAN = re.compile(r"\b(nhưng|tuy|mặc dù|song|có điều)\b", re.IGNORECASE)

_khoa = threading.Lock()
_trang_thai: dict = {"dang_chay": False, "tien_do": {}, "loi": {}, "bat_dau": None, "ket_thuc": None}


def trang_thai() -> dict:
    with _khoa:
        return json.loads(json.dumps(_trang_thai, default=str))


def _cap_nhat(**kw) -> None:
    with _khoa:
        _trang_thai.update(kw)


# ------------------------------------------------------------------ dữ liệu mẫu

def nap_mau(duong_dan: Path = FILE_MAU) -> dict:
    """Thêm mẫu từ file JSON vào dbo.MauKiemThu (bỏ qua mẫu trùng NoiDung + TapDuLieu)."""
    du_lieu = json.loads(duong_dan.read_text(encoding="utf-8"))
    hop_le_kc = set(du_lieu["khia_canh"])
    them = bo_qua = 0
    with get_connection() as conn:
        cur = conn.cursor()
        for m in du_lieu["mau"]:
            if m["cam_xuc"] not in ("TichCuc", "TrungTinh", "TieuCuc") or m["tap"] not in ("KiemThu", "ViDuMau"):
                raise ValueError(f"Nhãn không hợp lệ: {m}")
            if not set(m["khia_canh"]) <= hop_le_kc:
                raise ValueError(f"Khía cạnh không hợp lệ: {m}")
            cur.execute("SELECT 1 FROM dbo.MauKiemThu WHERE NoiDung = ? AND TapDuLieu = ?", m["noi_dung"], m["tap"])
            if cur.fetchone():
                bo_qua += 1
                continue
            cur.execute(
                "INSERT dbo.MauKiemThu (MaDanhGia, NoiDung, CamXucChuan, KhiaCanhChuan, TapDuLieu) VALUES (NULL, ?, ?, ?, ?)",
                m["noi_dung"], m["cam_xuc"], json.dumps(m["khia_canh"], ensure_ascii=False), m["tap"],
            )
            them += 1
        conn.commit()
    tong = truy_van("SELECT TapDuLieu, CamXucChuan, COUNT(*) AS SoMau FROM dbo.MauKiemThu "
                    "GROUP BY TapDuLieu, CamXucChuan ORDER BY TapDuLieu, CamXucChuan")
    return {"da_them": them, "bo_qua_trung": bo_qua, "thong_ke": tong}


# ------------------------------------------------------------------ phân loại lỗi

def nhom_loi(noi_dung: str, chuan: str, du_doan: str) -> str:
    if _TUONG_PHAN.search(noi_dung):
        return f"Câu tương phản (nhưng/tuy) – nhầm {chuan}→{du_doan}"[:100]
    if chuan == "TrungTinh":
        return f"Không nhận ra trung tính – đoán {du_doan}"[:100]
    if du_doan == "TrungTinh":
        return f"Đoán trung tính cho câu {chuan}"[:100]
    return f"Đảo cực cảm xúc {chuan}→{du_doan}"[:100]


# ------------------------------------------------------------------ chạy

def _chay_mot_mo_hinh(mo_hinh: str, mau: list[dict], khia_canh) -> None:
    bo = lay_bo_phan_tich(mo_hinh)
    for i, m in enumerate(mau, 1):
        t0 = time.perf_counter()
        try:
            kq = bo.phan_tich(m["NoiDung"], khia_canh)
        except Exception as e:  # noqa: BLE001 – một mẫu lỗi không dừng cả lượt chạy
            logger.warning("%s lỗi ở MauKiemThu#%s: %s", mo_hinh, m["MaMau"], e)
            with _khoa:
                _trang_thai["loi"][mo_hinh] = f"Mẫu #{m['MaMau']}: {e}"[:300]
            continue
        ms = int((time.perf_counter() - t0) * 1000)
        dung = kq.cam_xuc == m["CamXucChuan"]
        khia_canh_du_doan = json.dumps({k.ten: k.cam_xuc for k in kq.khia_canh}, ensure_ascii=False)
        with get_connection() as conn:
            conn.cursor().execute(
                """MERGE dbo.KetQuaThucNghiem AS t
                   USING (SELECT ? AS MaMau, ? AS MoHinh) AS s ON t.MaMau = s.MaMau AND t.MoHinh = s.MoHinh
                   WHEN MATCHED THEN UPDATE SET CamXucDuDoan = ?, KhiaCanhDuDoan = ?, LaDung = ?, NhomLoi = ?,
                        DoTreMs = ?, NgayChay = SYSUTCDATETIME()
                   WHEN NOT MATCHED THEN INSERT (MaMau, MoHinh, CamXucDuDoan, KhiaCanhDuDoan, LaDung, NhomLoi, DoTreMs)
                        VALUES (s.MaMau, s.MoHinh, ?, ?, ?, ?, ?);""",
                m["MaMau"], mo_hinh,
                kq.cam_xuc, khia_canh_du_doan, dung, None if dung else nhom_loi(m["NoiDung"], m["CamXucChuan"], kq.cam_xuc), ms,
                kq.cam_xuc, khia_canh_du_doan, dung, None if dung else nhom_loi(m["NoiDung"], m["CamXucChuan"], kq.cam_xuc), ms,
            )
            conn.commit()
        with _khoa:
            _trang_thai["tien_do"][mo_hinh] = {"da_xong": i, "tong": len(mau)}


def chay(cac_mo_hinh: list[str]) -> None:
    """Chạy đồng bộ (gọi trong BackgroundTasks). Cập nhật trạng thái để client thăm dò."""
    with _khoa:
        if _trang_thai["dang_chay"]:
            raise RuntimeError("Đang có một lượt thực nghiệm chạy.")
        _trang_thai.update(dang_chay=True, mo_hinh=cac_mo_hinh, tien_do={}, loi={},
                           bo_qua={}, bat_dau=datetime.now(timezone.utc), ket_thuc=None)
    try:
        mau = truy_van("SELECT MaMau, NoiDung, CamXucChuan FROM dbo.MauKiemThu WHERE TapDuLieu = 'KiemThu' ORDER BY MaMau")
        khia_canh = doc_khia_canh()
        for mo_hinh in cac_mo_hinh:
            if mo_hinh not in MO_HINH:
                continue
            try:
                lay_bo_phan_tich(mo_hinh)
            except MoHinhKhongSanSang as e:
                with _khoa:
                    _trang_thai["bo_qua"][mo_hinh] = str(e)
                logger.warning("Bỏ qua %s: %s", mo_hinh, e)
                continue
            with _khoa:
                _trang_thai["tien_do"][mo_hinh] = {"da_xong": 0, "tong": len(mau)}
            _chay_mot_mo_hinh(mo_hinh, mau, khia_canh)
    finally:
        _cap_nhat(dang_chay=False, ket_thuc=datetime.now(timezone.utc))
