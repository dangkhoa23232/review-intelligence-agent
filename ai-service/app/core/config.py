from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Cấu hình đọc từ biến môi trường / file .env."""

    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    app_env: str = "development"
    cors_origins: list[str] = ["http://localhost:5173"]

    qdrant_url: str = "http://localhost:6333"
    qdrant_api_key: str | None = None
    qdrant_collection: str = "danh_gia"  # payload liên kết DanhGia.MaDanhGia; point id = DanhGia.MaVectorQdrant

    embedding_model: str = "BAAI/bge-m3"
    embedding_dim: int = 1024

    sqlserver_connection_string: str = ""


@lru_cache
def get_settings() -> Settings:
    return Settings()
