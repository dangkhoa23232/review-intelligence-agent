import logging
import threading
from contextlib import asynccontextmanager

from fastapi import FastAPI, Request
from fastapi.exceptions import RequestValidationError
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from app.api import health, hoi_dap, phan_tich, thuc_nghiem
from app.core.config import get_settings

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s: %(message)s")

settings = get_settings()
logger = logging.getLogger("app")


def _nap_san_mo_hinh() -> None:
    """Nạp embedding + mô hình ABSA mặc định ngay khi khởi động để request đầu tiên không phải chờ."""
    try:
        from app.services.absa import mo_hinh_phan_tich
        from app.services.embedding import get_bo_nhung

        logger.info("Nạp sẵn embedding %s và mô hình ABSA %s.", get_bo_nhung().ten, mo_hinh_phan_tich())
    except Exception as e:  # noqa: BLE001 – không chặn khởi động; lỗi sẽ được báo ở /phan-tich/mo-hinh
        logger.warning("Không nạp sẵn được mô hình: %s", e)


@asynccontextmanager
async def lifespan(_: FastAPI):
    threading.Thread(target=_nap_san_mo_hinh, daemon=True, name="nap-mo-hinh").start()
    yield


app = FastAPI(
    title="Review Intelligence – AI Service",
    description="ABSA, tóm tắt ưu/nhược điểm, hỏi đáp review có dẫn chứng (RAG) và thực nghiệm mô hình.",
    version="0.2.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.cors_origins,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.exception_handler(RequestValidationError)
async def validation_error_handler(_: Request, exc: RequestValidationError) -> JSONResponse:
    """Lỗi validation đầu vào – trả 422 với danh sách lỗi theo từng trường."""
    return JSONResponse(
        status_code=422,
        content={
            "title": "Dữ liệu đầu vào không hợp lệ.",
            "errors": [{"truong": ".".join(map(str, e["loc"])), "thong_bao": e["msg"]} for e in exc.errors()],
        },
    )


app.include_router(health.router)
app.include_router(phan_tich.router)
app.include_router(hoi_dap.router)
app.include_router(thuc_nghiem.router)
