"""Kết nối SQL Server (ReviewIntelligenceDB) qua pyodbc.

Tên bảng/cột trong truy vấn phải khớp chính xác database/ReviewIntelligence_SQLServer.sql.
"""

from collections.abc import Iterator, Sequence
from contextlib import contextmanager
from typing import Any

import pyodbc

from app.core.config import get_settings


@contextmanager
def get_connection(timeout: int = 10, autocommit: bool = False) -> Iterator[pyodbc.Connection]:
    conn_str = get_settings().sqlserver_connection_string
    if not conn_str:
        raise RuntimeError("Chưa cấu hình SQLSERVER_CONNECTION_STRING.")
    conn = pyodbc.connect(conn_str, timeout=timeout, autocommit=autocommit)
    try:
        yield conn
    finally:
        conn.close()


def _dong_thanh_dict(cursor: pyodbc.Cursor, rows: Sequence[pyodbc.Row]) -> list[dict[str, Any]]:
    cot = [c[0] for c in cursor.description]
    return [dict(zip(cot, r)) for r in rows]


def truy_van(sql: str, *params: Any) -> list[dict[str, Any]]:
    """SELECT trả về list dict (tên cột → giá trị)."""
    with get_connection(autocommit=True) as conn:
        cur = conn.cursor()
        cur.execute(sql, *params)
        return _dong_thanh_dict(cur, cur.fetchall())


def truy_van_mot(sql: str, *params: Any) -> dict[str, Any] | None:
    ds = truy_van(sql, *params)
    return ds[0] if ds else None
