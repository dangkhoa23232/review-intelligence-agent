from fastapi import FastAPI, Request
from fastapi.exceptions import RequestValidationError
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from app.api import health
from app.core.config import get_settings

settings = get_settings()

app = FastAPI(
    title="Review Intelligence – AI Service",
    description="ABSA, tóm tắt ưu/nhược điểm và hỏi đáp review có dẫn chứng (RAG).",
    version="0.1.0",
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

# TODO: app/api/absa.py, app/api/tom_tat.py, app/api/hoi_dap.py (LangGraph agent)
