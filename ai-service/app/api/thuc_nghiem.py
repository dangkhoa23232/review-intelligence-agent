from fastapi import APIRouter, BackgroundTasks, HTTPException, status
from fastapi.concurrency import run_in_threadpool
from pydantic import BaseModel, Field

from app.services.absa.base import MO_HINH
from app.services.thuc_nghiem import chay, nap_mau, trang_thai

router = APIRouter(prefix="/thuc-nghiem", tags=["thuc-nghiem"])


class YeuCauChay(BaseModel):
    mo_hinh: list[str] = Field(default_factory=lambda: list(MO_HINH), min_length=1)


@router.post("/nap-mau")
async def nap() -> dict:
    """Nạp data/mau_kiem_thu.json vào dbo.MauKiemThu (bỏ qua mẫu đã có)."""
    return await run_in_threadpool(nap_mau)


@router.post("/chay", status_code=status.HTTP_202_ACCEPTED)
def bat_dau(req: YeuCauChay, background: BackgroundTasks) -> dict:
    sai = [m for m in req.mo_hinh if m not in MO_HINH]
    if sai:
        raise HTTPException(status.HTTP_422_UNPROCESSABLE_ENTITY, f"Mô hình không hợp lệ: {sai}. Chấp nhận: {list(MO_HINH)}")
    if trang_thai()["dang_chay"]:
        raise HTTPException(status.HTTP_409_CONFLICT, "Đang có một lượt thực nghiệm chạy.")
    background.add_task(chay, req.mo_hinh)
    return {"trang_thai": "da_tiep_nhan", "mo_hinh": req.mo_hinh}


@router.get("/trang-thai")
def xem_trang_thai() -> dict:
    return trang_thai()
