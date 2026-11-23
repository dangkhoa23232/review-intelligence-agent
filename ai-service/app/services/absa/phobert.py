"""PhoBERT – phân loại cảm xúc tiếng Việt (wonrax/phobert-base-vietnamese-sentiment: NEG/NEU/POS).

PhoBERT không có đầu ra khía cạnh, nên quy trình là:
1. Cảm xúc tổng: chạy PhoBERT trên toàn bộ đánh giá.
2. Khía cạnh: từ điển khía cạnh (app.services.van_ban) xác định mệnh đề nói về khía cạnh nào,
   rồi PhoBERT phân loại cảm xúc của chính mệnh đề đó.
Văn bản được tách từ bằng pyvi (PhoBERT được huấn luyện trên dữ liệu đã tách từ).
"""

import logging
import threading

from app.core.config import get_settings
from app.services.absa.base import KetQuaPhanTich, KhiaCanhDinhNghia, KhiaCanhPhanTich, MoHinhKhongSanSang
from app.services.thiet_bi import thiet_bi
from app.services.van_ban import chuan_hoa, doan_theo_khia_canh

logger = logging.getLogger(__name__)

_NHAN = {"NEG": "TieuCuc", "NEU": "TrungTinh", "POS": "TichCuc"}


class PhoBert:
    ten = "PhoBERT"

    def __init__(self) -> None:
        try:
            import torch
            from transformers import AutoModelForSequenceClassification, AutoTokenizer
        except ImportError as e:
            raise MoHinhKhongSanSang(f"Thiếu thư viện cho PhoBERT: {e}") from e

        ten_mo_hinh = get_settings().phobert_model
        try:
            self._tok = AutoTokenizer.from_pretrained(ten_mo_hinh)
            self._mo_hinh = AutoModelForSequenceClassification.from_pretrained(ten_mo_hinh)
        except Exception as e:  # noqa: BLE001
            raise MoHinhKhongSanSang(f"Không tải được {ten_mo_hinh}: {e}") from e

        self._torch = torch
        self._dev = thiet_bi()
        self._mo_hinh.to(self._dev).eval()
        self._id2label = self._mo_hinh.config.id2label
        self._khoa = threading.Lock()
        try:
            from pyvi import ViTokenizer

            self._tach_tu = ViTokenizer.tokenize
        except ImportError:
            logger.warning("Chưa cài pyvi – PhoBERT chạy trên văn bản chưa tách từ (độ chính xác thấp hơn).")
            self._tach_tu = lambda s: s
        logger.info("PhoBERT sẵn sàng trên %s.", self._dev)

    def _phan_loai(self, ds: list[str]) -> list[tuple[str, float]]:
        vao = [self._tach_tu(chuan_hoa(s)) for s in ds]
        with self._khoa, self._torch.no_grad():
            enc = self._tok(vao, padding=True, truncation=True, max_length=256, return_tensors="pt").to(self._dev)
            xs = self._torch.softmax(self._mo_hinh(**enc).logits, dim=-1).cpu()
        ket_qua = []
        for hang in xs:
            i = int(hang.argmax())
            ket_qua.append((_NHAN.get(str(self._id2label[i]).upper(), "TrungTinh"), float(hang[i])))
        return ket_qua

    def phan_tich(self, noi_dung: str, khia_canh: list[KhiaCanhDinhNghia]) -> KetQuaPhanTich:
        doan = doan_theo_khia_canh(noi_dung, [k.ten for k in khia_canh])
        ten_doan = list(doan)
        nhan = self._phan_loai([noi_dung, *[doan[t] for t in ten_doan]])
        cam_xuc, do_tin_cay = nhan[0]
        return KetQuaPhanTich(
            cam_xuc=cam_xuc,
            do_tin_cay=do_tin_cay,
            khia_canh=[KhiaCanhPhanTich(t, nhan[i + 1][0], doan[t]) for i, t in enumerate(ten_doan)],
        )
