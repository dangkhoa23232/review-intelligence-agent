from fastapi import APIRouter, BackgroundTasks, status
from pydantic import BaseModel, Field

from app.services.phan_tich import xu_ly_phan_tich_danh_gia

router = APIRouter(prefix="/phan-tich", tags=["phan-tich"])


class YeuCauPhanTichDanhGia(BaseModel):
    """Body do backend gửi khi một đánh giá vừa được duyệt (DanhGia.TrangThai = 'DaDuyet')."""

    ma_danh_gia: int = Field(gt=0, description="DanhGia.MaDanhGia")


class PhanHoiTiepNhan(BaseModel):
    trang_thai: str
    ma_danh_gia: int


@router.post("/danh-gia", status_code=status.HTTP_202_ACCEPTED, response_model=PhanHoiTiepNhan)
def tiep_nhan_phan_tich_danh_gia(req: YeuCauPhanTichDanhGia, background: BackgroundTasks) -> PhanHoiTiepNhan:
    """Nhận yêu cầu và xử lý nền (ABSA + embedding + Qdrant); trả 202 ngay để không chặn backend."""
    background.add_task(xu_ly_phan_tich_danh_gia, req.ma_danh_gia)
    return PhanHoiTiepNhan(trang_thai="da_tiep_nhan", ma_danh_gia=req.ma_danh_gia)
