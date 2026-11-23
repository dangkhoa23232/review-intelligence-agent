from dataclasses import dataclass, field
from typing import Protocol

CAM_XUC = ("TichCuc", "TrungTinh", "TieuCuc")  # CK_DanhGia_CamXuc / CK_KetQuaKhiaCanh_CamXuc
MO_HINH = ("PhoBERT", "LLM_KhongVD", "LLM_ItVD")  # CK_KetQuaKhiaCanh_MoHinh / CK_KetQuaThucNghiem_MoHinh


@dataclass
class KhiaCanhPhanTich:
    ten: str  # dbo.KhiaCanh.Ten
    cam_xuc: str  # TichCuc | TrungTinh | TieuCuc
    cum_tu: str | None = None  # KetQuaKhiaCanh.CumTuDanChung (≤ 300)


@dataclass
class KetQuaPhanTich:
    cam_xuc: str
    do_tin_cay: float  # 0..1 – DanhGia.DoTinCay
    khia_canh: list[KhiaCanhPhanTich] = field(default_factory=list)


@dataclass
class KhiaCanhDinhNghia:
    ma: int
    ten: str
    mo_ta: str | None


@dataclass
class ViDu:
    """Ví dụ few-shot lấy từ MauKiemThu (TapDuLieu = 'ViDuMau')."""

    noi_dung: str
    cam_xuc: str
    khia_canh: dict[str, str]


class BoPhanTich(Protocol):
    ten: str  # giá trị MoHinh ghi vào DB

    def phan_tich(self, noi_dung: str, khia_canh: list[KhiaCanhDinhNghia]) -> KetQuaPhanTich: ...


class MoHinhKhongSanSang(RuntimeError):
    """Mô hình không chạy được trên máy này (thiếu thư viện, mô hình hoặc khóa API)."""
