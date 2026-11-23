"""Chọn thiết bị chạy mô hình (GPU nếu có, ngược lại CPU) – an toàn khi chưa cài torch."""

from functools import lru_cache

from app.core.config import get_settings


@lru_cache
def thiet_bi() -> str:
    cau_hinh = get_settings().device
    if cau_hinh in ("cpu", "cuda"):
        return cau_hinh
    try:
        import torch

        return "cuda" if torch.cuda.is_available() else "cpu"
    except ImportError:
        return "cpu"
