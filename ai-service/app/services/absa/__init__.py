"""Đăng ký các cấu hình mô hình ABSA (giá trị MoHinh theo CHECK constraint của schema)."""

import logging
import threading

from app.core.config import get_settings
from app.db.sqlserver import truy_van
from app.services.absa.base import BoPhanTich, KhiaCanhDinhNghia, MoHinhKhongSanSang, ViDu

logger = logging.getLogger(__name__)

_cache: dict[str, BoPhanTich] = {}
_loi: dict[str, str] = {}
_khoa = threading.Lock()


def doc_khia_canh() -> list[KhiaCanhDinhNghia]:
    return [KhiaCanhDinhNghia(r["MaKhiaCanh"], r["Ten"], r["MoTa"])
            for r in truy_van("SELECT MaKhiaCanh, Ten, MoTa FROM dbo.KhiaCanh ORDER BY MaKhiaCanh")]


def doc_vi_du() -> list[ViDu]:
    import json

    rows = truy_van("SELECT NoiDung, CamXucChuan, KhiaCanhChuan FROM dbo.MauKiemThu "
                    "WHERE TapDuLieu = 'ViDuMau' ORDER BY MaMau")
    return [ViDu(r["NoiDung"], r["CamXucChuan"], json.loads(r["KhiaCanhChuan"] or "{}")) for r in rows]


def lay_bo_phan_tich(mo_hinh: str) -> BoPhanTich:
    """Khởi tạo (một lần) bộ phân tích; ném MoHinhKhongSanSang nếu không chạy được."""
    with _khoa:
        if mo_hinh in _cache:
            return _cache[mo_hinh]
        try:
            if mo_hinh == "PhoBERT":
                from app.services.absa.phobert import PhoBert

                bo: BoPhanTich = PhoBert()
            elif mo_hinh == "LLM_KhongVD":
                from app.services.absa.llm import LlmAbsa

                bo = LlmAbsa(None)
            elif mo_hinh == "LLM_ItVD":
                from app.services.absa.llm import LlmAbsa

                vi_du = doc_vi_du()
                if not vi_du:
                    raise MoHinhKhongSanSang("Chưa có mẫu ViDuMau trong MauKiemThu cho few-shot.")
                bo = LlmAbsa(vi_du)
            else:
                raise MoHinhKhongSanSang(f"Mô hình không hợp lệ: {mo_hinh}")
        except MoHinhKhongSanSang as e:
            _loi[mo_hinh] = str(e)
            raise
        _loi.pop(mo_hinh, None)
        _cache[mo_hinh] = bo
        return bo


def trang_thai_mo_hinh(thu_khoi_tao: bool = False) -> dict[str, dict]:
    kq = {}
    for m in ("PhoBERT", "LLM_KhongVD", "LLM_ItVD"):
        if thu_khoi_tao and m not in _cache:
            try:
                lay_bo_phan_tich(m)
            except MoHinhKhongSanSang:
                pass
        kq[m] = {"san_sang": m in _cache, "ly_do": _loi.get(m)}
    return kq


def mo_hinh_phan_tich() -> str:
    """Mô hình dùng khi phân tích đánh giá vừa duyệt."""
    cau_hinh = get_settings().mo_hinh_phan_tich
    if cau_hinh != "auto":
        return cau_hinh
    for m in ("LLM_ItVD", "PhoBERT"):
        try:
            lay_bo_phan_tich(m)
            return m
        except MoHinhKhongSanSang as e:
            logger.info("Bỏ qua %s: %s", m, e)
    raise MoHinhKhongSanSang("Không có mô hình ABSA nào chạy được (cần PhoBERT hoặc khóa Claude API).")
