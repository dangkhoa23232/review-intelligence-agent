"""ABSA bằng Claude: zero-shot (LLM_KhongVD) và few-shot (LLM_ItVD, ví dụ lấy từ MauKiemThu 'ViDuMau').

Đầu ra ràng buộc bằng structured outputs (output_config.format = JSON schema) nên luôn parse được.
"""

import json
import logging

import anthropic

from app.core.config import get_settings
from app.services.absa.base import (
    CAM_XUC,
    KetQuaPhanTich,
    KhiaCanhDinhNghia,
    KhiaCanhPhanTich,
    MoHinhKhongSanSang,
    ViDu,
)

logger = logging.getLogger(__name__)

_HUONG_DAN = """Bạn là hệ thống phân tích cảm xúc theo khía cạnh (ABSA) cho đánh giá mỹ phẩm tiếng Việt.

Nhiệm vụ với MỘT đánh giá:
1. cam_xuc: cảm xúc tổng thể của đánh giá – TichCuc, TrungTinh hoặc TieuCuc.
   Đánh giá có cả ý khen lẫn chê ở mức tương đương, hoặc chỉ nhận xét trung lập, là TrungTinh.
2. do_tin_cay: mức chắc chắn của bạn về cam_xuc, số thực từ 0 đến 1.
3. khia_canh: chỉ liệt kê các khía cạnh ĐƯỢC NHẮC TỚI trong danh sách cho trước. Với mỗi khía cạnh:
   - ten: đúng tên trong danh sách,
   - cam_xuc: cảm xúc của người viết về riêng khía cạnh đó,
   - cum_tu: cụm từ nguyên văn trong đánh giá làm dẫn chứng (ngắn gọn, không diễn giải lại).
Không suy đoán khía cạnh không được nhắc tới."""


def _schema(ten_khia_canh: list[str]) -> dict:
    return {
        "type": "object",
        "properties": {
            "cam_xuc": {"type": "string", "enum": list(CAM_XUC)},
            "do_tin_cay": {"type": "number"},
            "khia_canh": {
                "type": "array",
                "items": {
                    "type": "object",
                    "properties": {
                        "ten": {"type": "string", "enum": ten_khia_canh},
                        "cam_xuc": {"type": "string", "enum": list(CAM_XUC)},
                        "cum_tu": {"type": "string"},
                    },
                    "required": ["ten", "cam_xuc", "cum_tu"],
                    "additionalProperties": False,
                },
            },
        },
        "required": ["cam_xuc", "do_tin_cay", "khia_canh"],
        "additionalProperties": False,
    }


def tao_client() -> anthropic.Anthropic:
    """Client mặc định đọc ANTHROPIC_API_KEY / hồ sơ `ant auth login`."""
    s = get_settings()
    try:
        return anthropic.Anthropic(api_key=s.anthropic_api_key) if s.anthropic_api_key else anthropic.Anthropic()
    except Exception as e:  # noqa: BLE001 – không có thông tin xác thực
        raise MoHinhKhongSanSang(f"Chưa cấu hình khóa Claude API (ANTHROPIC_API_KEY): {e}") from e


def goi_claude(client: anthropic.Anthropic, *, system: str, noi_dung: str, effort: str, max_tokens: int,
               schema: dict | None = None) -> str:
    """Một lượt gọi Messages API; bật refusal fallback phía server; trả về text."""
    s = get_settings()
    output_config: dict = {"effort": effort}
    if schema is not None:
        output_config["format"] = {"type": "json_schema", "schema": schema}
    res = client.beta.messages.create(
        model=s.llm_model,
        max_tokens=max_tokens,
        betas=["server-side-fallback-2026-07-01"],
        fallbacks="default",
        system=system,
        output_config=output_config,
        messages=[{"role": "user", "content": noi_dung}],
    )
    if res.stop_reason == "refusal":
        loai = res.stop_details.category if res.stop_details else None
        raise RuntimeError(f"Claude từ chối yêu cầu (category={loai}).")
    if res.stop_reason == "max_tokens":
        raise RuntimeError("Phản hồi Claude bị cắt do max_tokens.")
    return "".join(b.text for b in res.content if b.type == "text")


class LlmAbsa:
    def __init__(self, vi_du: list[ViDu] | None) -> None:
        self.ten = "LLM_ItVD" if vi_du else "LLM_KhongVD"
        self._vi_du = vi_du or []
        self._client = tao_client()
        # Kiểm tra thông tin xác thực có tồn tại mà không tốn token.
        if not (get_settings().anthropic_api_key or self._client.api_key or getattr(self._client, "auth_token", None)):
            raise MoHinhKhongSanSang("Chưa cấu hình khóa Claude API (ANTHROPIC_API_KEY).")

    def _system(self, khia_canh: list[KhiaCanhDinhNghia]) -> str:
        ds = "\n".join(f"- {k.ten}: {k.mo_ta or ''}".rstrip(": ") for k in khia_canh)
        phan = [_HUONG_DAN, f"Danh sách khía cạnh:\n{ds}"]
        if self._vi_du:
            mau = "\n\n".join(
                f"Đánh giá: {v.noi_dung}\nKết quả: "
                + json.dumps({"cam_xuc": v.cam_xuc,
                              "khia_canh": [{"ten": t, "cam_xuc": c} for t, c in v.khia_canh.items()]},
                             ensure_ascii=False)
                for v in self._vi_du
            )
            phan.append(f"Một số ví dụ đã gán nhãn:\n\n{mau}")
        return "\n\n".join(phan)

    def phan_tich(self, noi_dung: str, khia_canh: list[KhiaCanhDinhNghia]) -> KetQuaPhanTich:
        ten = [k.ten for k in khia_canh]
        text = goi_claude(
            self._client,
            system=self._system(khia_canh),
            noi_dung=f"Đánh giá:\n{noi_dung}",
            effort=get_settings().llm_effort_phan_tich,
            max_tokens=8000,
            schema=_schema(ten),
        )
        d = json.loads(text)
        return KetQuaPhanTich(
            cam_xuc=d["cam_xuc"],
            do_tin_cay=min(1.0, max(0.0, float(d["do_tin_cay"]))),
            khia_canh=[
                KhiaCanhPhanTich(x["ten"], x["cam_xuc"], (x.get("cum_tu") or "")[:300] or None)
                for x in d["khia_canh"]
                if x["ten"] in ten
            ],
        )
