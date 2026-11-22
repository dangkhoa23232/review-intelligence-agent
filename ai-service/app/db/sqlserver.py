"""Kết nối SQL Server (ReviewIntelligenceDB) qua pyodbc.

Tên bảng/cột trong truy vấn phải khớp chính xác database/ReviewIntelligence_SQLServer.sql.
"""

from collections.abc import Iterator
from contextlib import contextmanager

import pyodbc

from app.core.config import get_settings


@contextmanager
def get_connection(timeout: int = 5) -> Iterator[pyodbc.Connection]:
    conn_str = get_settings().sqlserver_connection_string
    if not conn_str:
        raise RuntimeError("Chưa cấu hình SQLSERVER_CONNECTION_STRING.")
    conn = pyodbc.connect(conn_str, timeout=timeout)
    try:
        yield conn
    finally:
        conn.close()
