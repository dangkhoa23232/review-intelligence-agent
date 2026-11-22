from datetime import datetime, timezone

from fastapi import APIRouter, Response, status
from pydantic import BaseModel

from app.core.config import get_settings
from app.db.qdrant import get_qdrant
from app.db.sqlserver import get_connection

router = APIRouter(tags=["health"])


class ThanhPhan(BaseModel):
    ok: bool
    chi_tiet: str | None = None


class HealthResponse(BaseModel):
    trang_thai: str
    thoi_gian: datetime
    qdrant: ThanhPhan
    sql_server: ThanhPhan


def _kiem_tra_qdrant() -> ThanhPhan:
    try:
        collections = [c.name for c in get_qdrant().get_collections().collections]
        coll = get_settings().qdrant_collection
        return ThanhPhan(ok=True, chi_tiet=f"collection '{coll}' {'đã có' if coll in collections else 'chưa tạo'}")
    except Exception as e:  # noqa: BLE001 – health-check chỉ báo trạng thái
        return ThanhPhan(ok=False, chi_tiet=str(e))


def _kiem_tra_sql_server() -> ThanhPhan:
    try:
        with get_connection(timeout=3) as conn:
            conn.cursor().execute("SELECT 1").fetchone()
        return ThanhPhan(ok=True)
    except Exception as e:  # noqa: BLE001
        return ThanhPhan(ok=False, chi_tiet=str(e))


@router.get("/health", response_model=HealthResponse)
def health(response: Response) -> HealthResponse:
    """Kiểm tra service và các kết nối phụ thuộc (Qdrant, SQL Server). Trả 503 nếu có thành phần lỗi."""
    qdrant = _kiem_tra_qdrant()
    sql = _kiem_tra_sql_server()
    ok = qdrant.ok and sql.ok
    if not ok:
        response.status_code = status.HTTP_503_SERVICE_UNAVAILABLE
    return HealthResponse(
        trang_thai="ok" if ok else "loi",
        thoi_gian=datetime.now(timezone.utc),
        qdrant=qdrant,
        sql_server=sql,
    )
