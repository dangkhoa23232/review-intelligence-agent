"""Vector nhúng cho đánh giá và câu hỏi.

- "bge-m3": BAAI/bge-m3 (dense 1024 chiều, đa ngôn ngữ, tốt cho tiếng Việt) – mặc định.
- "hash":   băm n-gram âm tiết vào 1024 chiều (truy hồi theo từ vựng). Chỉ là phương án dự phòng
            khi chưa cài sentence-transformers / torch – chất lượng ngữ nghĩa thấp hơn rõ rệt.
"""

import hashlib
import logging
import math
import re
import threading
from functools import lru_cache
from typing import Protocol

from app.core.config import get_settings
from app.services.thiet_bi import thiet_bi
from app.services.van_ban import chuan_hoa

logger = logging.getLogger(__name__)


class BoNhung(Protocol):
    ten: str
    so_chieu: int

    def nhung(self, van_ban: list[str]) -> list[list[float]]: ...


class BgeM3:
    ten = "bge-m3"
    so_chieu = 1024

    def __init__(self, ten_mo_hinh: str) -> None:
        import torch
        from sentence_transformers import SentenceTransformer

        dev = thiet_bi()
        kwargs = {"torch_dtype": torch.float16} if dev == "cuda" else {}
        logger.info("Đang tải %s trên %s…", ten_mo_hinh, dev)
        self._mo_hinh = SentenceTransformer(ten_mo_hinh, device=dev, model_kwargs=kwargs)
        self._khoa = threading.Lock()

    def nhung(self, van_ban: list[str]) -> list[list[float]]:
        with self._khoa:
            v = self._mo_hinh.encode(van_ban, normalize_embeddings=True, batch_size=16, show_progress_bar=False)
        return [list(map(float, x)) for x in v]


class NhungBam:
    """Băm âm tiết + cặp âm tiết (signed hashing) rồi chuẩn hóa L2."""

    ten = "hash"

    def __init__(self, so_chieu: int) -> None:
        self.so_chieu = so_chieu

    def _mot(self, s: str) -> list[float]:
        am_tiet = re.findall(r"\w+", chuan_hoa(s).lower())
        dac_trung = am_tiet + [f"{a}_{b}" for a, b in zip(am_tiet, am_tiet[1:])]
        v = [0.0] * self.so_chieu
        for f in dac_trung:
            h = int(hashlib.md5(f.encode("utf-8")).hexdigest(), 16)
            v[h % self.so_chieu] += 1.0 if (h >> 64) & 1 else -1.0
        do_dai = math.sqrt(sum(x * x for x in v)) or 1.0
        return [x / do_dai for x in v]

    def nhung(self, van_ban: list[str]) -> list[list[float]]:
        return [self._mot(s) for s in van_ban]


_ly_do_du_phong: str | None = None


@lru_cache
def get_bo_nhung() -> BoNhung:
    global _ly_do_du_phong
    s = get_settings()
    if s.embedding_backend == "bge-m3":
        try:
            return BgeM3(s.embedding_model)
        except Exception as e:  # noqa: BLE001 – thiếu thư viện / không tải được mô hình
            _ly_do_du_phong = f"Không tải được {s.embedding_model}: {e}"
            logger.warning("%s → dùng embedding băm dự phòng.", _ly_do_du_phong)
    return NhungBam(s.embedding_dim_hash)


def ly_do_du_phong() -> str | None:
    return _ly_do_du_phong
