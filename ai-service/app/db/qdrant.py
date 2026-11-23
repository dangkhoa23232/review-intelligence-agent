"""Kho vector Qdrant cho đánh giá đã duyệt.

- point id   = DanhGia.MaVectorQdrant (UUID)
- payload    = ma_danh_gia, ma_san_pham, so_sao, cam_xuc, noi_dung
- collection = <QDRANT_COLLECTION>__<embedding> để đổi embedding không làm hỏng dữ liệu cũ.
"""

import logging
import threading
from functools import lru_cache
from typing import Any

from qdrant_client import QdrantClient
from qdrant_client.http import models as qm

from app.core.config import get_settings

logger = logging.getLogger(__name__)
_khoa = threading.Lock()  # chế độ local không an toàn đa luồng → tuần tự hóa truy cập


@lru_cache
def get_qdrant() -> QdrantClient:
    s = get_settings()
    if s.qdrant_mode == "server":
        return QdrantClient(url=s.qdrant_url, api_key=s.qdrant_api_key or None, timeout=10)
    return QdrantClient(path=s.qdrant_path)


def ten_collection(ten_embedding: str) -> str:
    return f"{get_settings().qdrant_collection}__{ten_embedding.replace('-', '_')}"


def dam_bao_collection(ten: str, so_chieu: int) -> None:
    with _khoa:
        client = get_qdrant()
        if not client.collection_exists(ten):
            client.create_collection(ten, vectors_config=qm.VectorParams(size=so_chieu, distance=qm.Distance.COSINE))
            logger.info("Đã tạo collection Qdrant %s (%s chiều).", ten, so_chieu)


def upsert(ten: str, point_id: str, vector: list[float], payload: dict[str, Any]) -> None:
    with _khoa:
        get_qdrant().upsert(ten, points=[qm.PointStruct(id=point_id, vector=vector, payload=payload)])


def xoa(ten: str, point_id: str) -> None:
    with _khoa:
        client = get_qdrant()
        if client.collection_exists(ten):
            client.delete(ten, points_selector=qm.PointIdsList(points=[point_id]))


def tim(ten: str, vector: list[float], gioi_han: int, ma_san_pham: int | None) -> list[qm.ScoredPoint]:
    loc = None
    if ma_san_pham is not None:
        loc = qm.Filter(must=[qm.FieldCondition(key="ma_san_pham", match=qm.MatchValue(value=ma_san_pham))])
    with _khoa:
        client = get_qdrant()
        if not client.collection_exists(ten):
            return []
        return client.query_points(ten, query=vector, query_filter=loc, limit=gioi_han, with_payload=True).points


def ton_tai(ten: str, point_ids: list[str]) -> set[str]:
    """Các point id (trong danh sách) đang có trong collection."""
    if not point_ids:
        return set()
    with _khoa:
        client = get_qdrant()
        if not client.collection_exists(ten):
            return set()
        return {str(p.id) for p in client.retrieve(ten, ids=point_ids, with_payload=False, with_vectors=False)}


def so_diem(ten: str) -> int:
    with _khoa:
        client = get_qdrant()
        return client.count(ten).count if client.collection_exists(ten) else 0
