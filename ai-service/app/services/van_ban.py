"""Xử lý văn bản tiếng Việt: tách câu/mệnh đề và nhận diện khía cạnh bằng từ điển.

Từ điển chỉ dùng để XÁC ĐỊNH đoạn văn nói về khía cạnh nào (cho PhoBERT – mô hình không có đầu ra
khía cạnh) và để lọc dẫn chứng RAG; cảm xúc luôn do mô hình (PhoBERT / LLM) quyết định.
Khóa là tên khía cạnh trong dbo.KhiaCanh.Ten.
"""

import re
import unicodedata

TU_KHOA_KHIA_CANH: dict[str, tuple[str, ...]] = {
    "Mùi hương": ("mùi", "thơm", "hương", "hắc", "nồng", "không mùi", "nặng mùi"),
    "Độ thấm": ("thấm", "bết", "dính", "nhờn", "nhớt", "khô thoáng", "ráo", "kết cấu", "texture", "mỏng nhẹ",
                "đặc sệt", "lỏng", "bóng dầu", "cakey", "vón"),
    "Bao bì": ("bao bì", "chai", "vòi", "nắp", "hộp", "tuýp", "lọ", "đóng gói", "rò", "thiết kế", "vỏ",
               "móp", "bể", "vỡ", "seal"),
    "Giá": ("giá", "tiền", "đắt", "rẻ", "đáng tiền", "chi phí", "khuyến mãi", "sale", "túi tiền"),
    "Kích ứng": ("kích ứng", "châm chích", "rát", "mẩn", "ngứa", "nổi mụn", "dị ứng", "sần", "bỏng", "lành tính",
                 "dịu nhẹ", "dịu da", "ửng đỏ", "đỏ rát", "nổi mẩn", "bong tróc"),
    "Hiệu quả": ("hiệu quả", "mờ thâm", "thâm", "vết thâm", "giảm mụn", "sạm", "cháy nắng", "bị cháy", "kiềm dầu", "dưỡng ẩm", "sáng da", "cải thiện", "se khít",
                 "mịn", "căng", "chống nắng", "hết mụn", "công dụng", "tác dụng", "đỡ mụn", "xẹp mụn", "phục hồi",
                 "làm sạch", "sạch", "đều màu", "không thấy thay đổi", "chẳng thấy gì", "ẩm"),
}

_TACH_CAU = re.compile(r"[.!?;\n]+")
_TACH_MENH_DE = re.compile(r",|\bnhưng\b|\btuy nhiên\b|\bsong\b|\bcòn\b|\bmà\b", re.IGNORECASE)


def chuan_hoa(s: str) -> str:
    return unicodedata.normalize("NFC", s).strip()


def tach_cau(van_ban: str) -> list[str]:
    return [c.strip() for c in _TACH_CAU.split(chuan_hoa(van_ban)) if c.strip()]


def tach_menh_de(van_ban: str) -> list[str]:
    ket_qua: list[str] = []
    for cau in tach_cau(van_ban):
        ket_qua.extend(m.strip(" -–") for m in _TACH_MENH_DE.split(cau) if m and m.strip(" -–"))
    return ket_qua


def _co_tu(van_ban_thuong: str, tu: str) -> bool:
    # So khớp theo ranh giới âm tiết để "giá" không khớp "đánh giá" quá dễ dãi.
    if tu == "giá":
        return re.search(r"(?<!đánh )\bgiá\b", van_ban_thuong) is not None
    return re.search(rf"(?<!\w){re.escape(tu)}(?!\w)", van_ban_thuong) is not None


def khia_canh_cua(van_ban: str, ten_hop_le: list[str] | None = None) -> list[str]:
    thuong = chuan_hoa(van_ban).lower()
    ds = ten_hop_le if ten_hop_le is not None else list(TU_KHOA_KHIA_CANH)
    return [ten for ten in ds if any(_co_tu(thuong, t) for t in TU_KHOA_KHIA_CANH.get(ten, ()))]


def doan_theo_khia_canh(van_ban: str, ten_hop_le: list[str]) -> dict[str, str]:
    """Ghép các mệnh đề liền kề cùng nói về một khía cạnh → {tên khía cạnh: cụm từ dẫn chứng}."""
    ket_qua: dict[str, list[str]] = {}
    for cau in tach_cau(van_ban):
        menh_de = [m.strip(" -–") for m in _TACH_MENH_DE.split(cau) if m and m.strip(" -–")]
        for i, md in enumerate(menh_de):
            for ten in khia_canh_cua(md, ten_hop_le):
                ds = ket_qua.setdefault(ten, [])
                ds.append(md)
                # Mệnh đề ngắn ngay sau (cùng câu) không nói khía cạnh nào → phần tiếp nối
                # (vd "châm chích, nổi mẩn đỏ", "mùi hơi hắc, chấp nhận được").
                if i + 1 < len(menh_de) and not khia_canh_cua(menh_de[i + 1], ten_hop_le) and len(menh_de[i + 1]) < 40:
                    ds.append(menh_de[i + 1])
    return {ten: ", ".join(dict.fromkeys(ds))[:300] for ten, ds in ket_qua.items()}


def cau_lien_quan_nhat(van_ban: str, truy_van: str, khia_canh: list[str]) -> str:
    """Câu trong đánh giá liên quan nhất tới câu hỏi – làm đoạn trích dẫn chứng."""
    cau = tach_cau(van_ban) or [van_ban]
    tu_hoi = set(re.findall(r"\w+", truy_van.lower()))

    def diem(c: str) -> float:
        thuong = c.lower()
        d = len(tu_hoi & set(re.findall(r"\w+", thuong)))
        d += 3 * len(khia_canh_cua(c, khia_canh)) if khia_canh else 0
        return d

    return max(cau, key=diem)[:300]
