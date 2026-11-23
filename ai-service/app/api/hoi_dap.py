import uuid

from fastapi import APIRouter, HTTPException, status
from fastapi.concurrency import run_in_threadpool
from pydantic import BaseModel, Field, field_validator

from app.services.hoi_dap import KhongTimThaySanPham, hoi

router = APIRouter(tags=["hoi-dap"])


class YeuCauHoiDap(BaseModel):
    cau_hoi: str = Field(min_length=2, max_length=500)
    ma_san_pham: int | None = Field(default=None, gt=0)
    ma_nguoi_dung: int | None = Field(default=None, gt=0)
    ma_phien: str | None = None

    @field_validator("cau_hoi")
    @classmethod
    def _khong_rong(cls, v: str) -> str:
        v = v.strip()
        if len(v) < 2:
            raise ValueError("Câu hỏi quá ngắn.")
        return v

    @field_validator("ma_phien")
    @classmethod
    def _la_uuid(cls, v: str | None) -> str | None:
        if v is None:
            return None
        return str(uuid.UUID(v))


@router.post("/hoi-dap")
async def hoi_dap(req: YeuCauHoiDap) -> dict:
    """Hỏi đáp RAG có dẫn chứng; lưu TinNhan + NhatKyTacTu. Trả về câu trả lời, dẫn chứng và trace."""
    try:
        return await run_in_threadpool(hoi, req.cau_hoi, req.ma_san_pham, req.ma_nguoi_dung, req.ma_phien)
    except KhongTimThaySanPham as e:
        raise HTTPException(status.HTTP_404_NOT_FOUND, str(e)) from e
