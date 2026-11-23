import logging
import threading

from fastapi import APIRouter, BackgroundTasks, Path, status
from pydantic import BaseModel, Field

from app.services.absa import trang_thai_mo_hinh
from app.services.embedding import get_bo_nhung, ly_do_du_phong
from app.services.phan_tich import danh_sach_can_dong_bo, go_bo_danh_gia, phan_tich_danh_gia

logger = logging.getLogger(__name__)
router = APIRouter(prefix="/phan-tich", tags=["phan-tich"])

# Tuần tự hóa xử lý nền để không nạp mô hình / ghi DB song song cho cùng dữ liệu.
_khoa_xu_ly = threading.Lock()


class YeuCauPhanTichDanhGia(BaseModel):
    """Body do backend gửi khi một đánh giá vừa được duyệt (DanhGia.TrangThai = 'DaDuyet')."""

    ma_danh_gia: int = Field(gt=0, description="DanhGia.MaDanhGia")


class PhanHoiTiepNhan(BaseModel):
    trang_thai: str
    ma_danh_gia: int | None = None
    so_danh_gia: int | None = None


def _an_toan(ten: str, fn, *args) -> None:
    with _khoa_xu_ly:
        try:
            fn(*args)
        except Exception:  # noqa: BLE001 – tác vụ nền không được làm sập service
            logger.exception("%s thất bại với tham số %s.", ten, args)


def _dong_bo(ds: list[int]) -> None:
    for ma in ds:
        _an_toan("Phân tích", phan_tich_danh_gia, ma)


@router.post("/danh-gia", status_code=status.HTTP_202_ACCEPTED, response_model=PhanHoiTiepNhan)
def tiep_nhan_phan_tich(req: YeuCauPhanTichDanhGia, background: BackgroundTasks) -> PhanHoiTiepNhan:
    """Phân tích nền: ABSA → KetQuaKhiaCanh, DanhGia.CamXuc/DoTinCay/MaVectorQdrant, Qdrant, TomTatKhiaCanh."""
    background.add_task(_an_toan, "Phân tích", phan_tich_danh_gia, req.ma_danh_gia)
    return PhanHoiTiepNhan(trang_thai="da_tiep_nhan", ma_danh_gia=req.ma_danh_gia)


@router.delete("/danh-gia/{ma_danh_gia}", status_code=status.HTTP_202_ACCEPTED, response_model=PhanHoiTiepNhan)
def tiep_nhan_go_bo(background: BackgroundTasks, ma_danh_gia: int = Path(gt=0)) -> PhanHoiTiepNhan:
    """Đánh giá chuyển sang DaAn: xóa vector khỏi Qdrant và tính lại TomTatKhiaCanh."""
    background.add_task(_an_toan, "Gỡ bỏ", go_bo_danh_gia, ma_danh_gia)
    return PhanHoiTiepNhan(trang_thai="da_tiep_nhan", ma_danh_gia=ma_danh_gia)


@router.post("/dong-bo", status_code=status.HTTP_202_ACCEPTED, response_model=PhanHoiTiepNhan)
def dong_bo(background: BackgroundTasks, tat_ca: bool = False) -> PhanHoiTiepNhan:
    """Phân tích các đánh giá DaDuyet còn thiếu kết quả/vector (tat_ca=true: phân tích lại toàn bộ)."""
    ds = danh_sach_can_dong_bo(tat_ca)
    background.add_task(_dong_bo, ds)
    return PhanHoiTiepNhan(trang_thai="da_tiep_nhan", so_danh_gia=len(ds))


@router.get("/mo-hinh")
def mo_hinh(thu_khoi_tao: bool = False) -> dict:
    """Mô hình ABSA / embedding nào đang chạy được trên máy này."""
    nhung = get_bo_nhung() if thu_khoi_tao else None
    return {
        "mo_hinh": trang_thai_mo_hinh(thu_khoi_tao),
        "embedding": nhung.ten if nhung else None,
        "embedding_du_phong": ly_do_du_phong(),
    }
