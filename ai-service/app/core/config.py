from functools import lru_cache
from pathlib import Path

from pydantic_settings import BaseSettings, SettingsConfigDict

THU_MUC_GOC = Path(__file__).resolve().parents[2]  # ai-service/


class Settings(BaseSettings):
    """Cấu hình đọc từ biến môi trường / file .env (xem .env.example)."""

    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    app_env: str = "development"
    cors_origins: list[str] = ["http://localhost:5173"]

    # ---------- SQL Server ----------
    sqlserver_connection_string: str = ""

    # ---------- Qdrant ----------
    # "local": nhúng trong tiến trình, lưu trên đĩa (không cần Docker) – "server": kết nối Qdrant server.
    qdrant_mode: str = "local"
    qdrant_path: str = str(THU_MUC_GOC / "qdrant_local")
    qdrant_url: str = "http://localhost:6333"
    qdrant_api_key: str | None = None
    qdrant_collection: str = "danh_gia"  # tên thật = <collection>__<embedding>

    # ---------- Embedding ----------
    # "bge-m3": BAAI/bge-m3 qua sentence-transformers (khuyến nghị) – "hash": dự phòng không cần mô hình.
    embedding_backend: str = "bge-m3"
    embedding_model: str = "BAAI/bge-m3"
    embedding_dim_hash: int = 1024

    # ---------- Mô hình ABSA ----------
    phobert_model: str = "wonrax/phobert-base-vietnamese-sentiment"
    # Thiết bị cho PhoBERT / BGE-M3: "auto" (cuda nếu có), "cuda", "cpu".
    device: str = "auto"
    # Mô hình dùng khi phân tích đánh giá vừa duyệt: "auto" = LLM_ItVD nếu có khóa API, ngược lại PhoBERT.
    mo_hinh_phan_tich: str = "auto"

    # ---------- LLM (Claude) ----------
    # Khóa đọc từ ANTHROPIC_API_KEY (hoặc hồ sơ `ant auth login`) – không đặt trong code.
    anthropic_api_key: str | None = None
    llm_model: str = "claude-opus-5-5"
    llm_effort_phan_tich: str = "low"
    llm_effort_tra_loi: str = "medium"

    # ---------- Hỏi đáp RAG ----------
    rag_top_k: int = 6
    # Ngưỡng điểm tương đồng tối thiểu để coi là dẫn chứng (cosine). Embedding "hash" cho điểm thấp hơn BGE-M3.
    rag_nguong_bge: float = 0.45
    rag_nguong_hash: float = 0.12


@lru_cache
def get_settings() -> Settings:
    return Settings()
