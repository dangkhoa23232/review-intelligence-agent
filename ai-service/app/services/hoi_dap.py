"""Tác tử hỏi đáp RAG có dẫn chứng (LangGraph).

Đồ thị:  lap_ke_hoach ─┬─(ngoài phạm vi)──────────────────────────────► kiem_soat ─► tra_loi
                       └─► tim_danh_gia ─► thong_ke_khia_canh ─► quan_sat ─┘
Mỗi bước được ghi vào dbo.NhatKyTacTu (LoaiBuoc theo CK_NhatKyTacTu_LoaiBuoc); câu hỏi và câu trả lời
lưu ở dbo.TinNhan (NguoiGui = 'NguoiDung' / 'TroLy', DanChung là JSON, DoTreMs).
Câu trả lời chỉ dựa trên đánh giá DaDuyet: ngoài phạm vi → từ chối lịch sự; không có dẫn chứng → báo
chưa có đánh giá nhắc tới; có dẫn chứng → trả lời kèm trích dẫn [n].
"""

import json
import logging
import operator
import re
import time
import uuid
from typing import Annotated, Any, TypedDict

from langgraph.graph import END, START, StateGraph

from app.core.config import get_settings
from app.db import qdrant
from app.db.sqlserver import get_connection, truy_van, truy_van_mot
from app.services.absa.base import MoHinhKhongSanSang
from app.services.embedding import get_bo_nhung
from app.services.van_ban import cau_lien_quan_nhat, khia_canh_cua

logger = logging.getLogger(__name__)

_TU_PHAM_VI = (
    "da", "sản phẩm", "kem", "serum", "tinh chất", "dùng", "xài", "bôi", "mụn", "thâm", "review", "đánh giá",
    "có nên", "tốt không", "có tốt", "phù hợp", "mua", "mỹ phẩm", "skincare", "chống nắng", "rửa mặt",
    "dưỡng", "nhạy cảm", "da dầu", "da khô", "người dùng", "khách hàng", "ưu điểm", "nhược điểm", "điểm cộng",
    "điểm trừ", "chất lượng", "sữa rửa mặt", "toner", "mặt nạ",
)
# Tên loại sản phẩm – không coi là khía cạnh khi xuất hiện trong câu hỏi ("kem chống nắng này…").
_LOAI_SAN_PHAM = ("kem chống nắng", "gel rửa mặt", "sữa rửa mặt", "tinh chất", "kem dưỡng", "serum", "toner")


def _bo_ten_san_pham(cau: str, ten_san_pham: str | None) -> str:
    s = cau.lower()
    for cum in ([ten_san_pham.lower()] if ten_san_pham else []) + list(_LOAI_SAN_PHAM):
        s = s.replace(cum, " ")
    return s


class TrangThai(TypedDict, total=False):
    cau_hoi: str
    ma_san_pham: int | None
    ten_san_pham: str | None
    khia_canh_hop_le: list[str]
    ke_hoach: dict[str, Any]
    ung_vien: list[dict[str, Any]]
    thong_ke: list[dict[str, Any]]
    dan_chung: list[dict[str, Any]]
    che_do: str  # tu_choi | thieu_dan_chung | tra_loi
    tra_loi: str
    che_do_sinh: str  # llm | mau
    buoc: Annotated[list[dict[str, Any]], operator.add]


def _buoc(loai: str, t0: float, dau_vao: Any = None, dau_ra: Any = None, cong_cu: str | None = None) -> dict:
    return {"loai_buoc": loai, "ten_cong_cu": cong_cu, "dau_vao": dau_vao, "dau_ra": dau_ra,
            "do_tre_ms": int((time.perf_counter() - t0) * 1000)}


def _nguong() -> float:
    s = get_settings()
    return s.rag_nguong_bge if get_bo_nhung().ten == "bge-m3" else s.rag_nguong_hash


# ------------------------------------------------------------------ các nút

def lap_ke_hoach(st: TrangThai) -> dict:
    t0 = time.perf_counter()
    cau = st["cau_hoi"]
    khia_canh = khia_canh_cua(_bo_ten_san_pham(cau, st.get("ten_san_pham")), st["khia_canh_hop_le"])
    thuong = cau.lower()
    pham_vi = bool(khia_canh) or any(re.search(rf"(?<!\w){re.escape(t)}(?!\w)", thuong) for t in _TU_PHAM_VI)
    if not pham_vi and st.get("ten_san_pham"):
        pham_vi = any(w in thuong for w in re.findall(r"\w{3,}", st["ten_san_pham"].lower()))
    ke_hoach = {
        "pham_vi": pham_vi,
        "khia_canh": khia_canh,
        "truy_van": cau if not khia_canh else f"{cau} ({', '.join(khia_canh)})",
        "cong_cu": ["tim_danh_gia", "thong_ke_khia_canh"] if pham_vi else [],
    }
    return {"ke_hoach": ke_hoach,
            "buoc": [_buoc("LapKeHoach", t0, {"cau_hoi": cau, "ma_san_pham": st.get("ma_san_pham")}, ke_hoach)]}


def tim_danh_gia(st: TrangThai) -> dict:
    t0 = time.perf_counter()
    s = get_settings()
    kh = st["ke_hoach"]
    nhung = get_bo_nhung()
    diem = qdrant.tim(qdrant.ten_collection(nhung.ten), nhung.nhung([kh["truy_van"]])[0],
                      s.rag_top_k * 2, st.get("ma_san_pham"))
    theo_ma: dict[int, dict] = {int(p.payload["ma_danh_gia"]): {"diem": float(p.score), "nguon": "vector"} for p in diem}

    # Lai ghép: đánh giá đã được ABSA gán đúng khía cạnh được hỏi luôn là ứng viên.
    if kh["khia_canh"] and st.get("ma_san_pham"):
        dau_hoi = ",".join("?" * len(kh["khia_canh"]))
        for r in truy_van(
            f"""SELECT DISTINCT x.MaDanhGia FROM dbo.KetQuaKhiaCanh x
                JOIN dbo.KhiaCanh k ON k.MaKhiaCanh = x.MaKhiaCanh
                JOIN dbo.DanhGia dg ON dg.MaDanhGia = x.MaDanhGia AND dg.TrangThai = 'DaDuyet'
                WHERE dg.MaSanPham = ? AND k.Ten IN ({dau_hoi})""",
            st["ma_san_pham"], *kh["khia_canh"],
        ):
            theo_ma.setdefault(r["MaDanhGia"], {"diem": _nguong(), "nguon": "khia_canh"})["nguon_khia_canh"] = True

    ung_vien: list[dict] = []
    if theo_ma:
        dau_hoi = ",".join("?" * len(theo_ma))
        for r in truy_van(
            f"""SELECT dg.MaDanhGia, dg.MaSanPham, dg.NoiDung, dg.SoSao, dg.DaMuaHang, nd.HoTen, sp.Ten AS TenSanPham
                FROM dbo.DanhGia dg
                JOIN dbo.NguoiDung nd ON nd.MaNguoiDung = dg.MaNguoiDung
                JOIN dbo.SanPham sp ON sp.MaSanPham = dg.MaSanPham
                WHERE dg.TrangThai = 'DaDuyet' AND dg.MaDanhGia IN ({dau_hoi})""",
            *theo_ma.keys(),
        ):
            m = theo_ma[r["MaDanhGia"]]
            noi_khia_canh = bool(m.get("nguon_khia_canh")) or bool(khia_canh_cua(r["NoiDung"], kh["khia_canh"]))
            ung_vien.append({
                "ma_danh_gia": r["MaDanhGia"], "ma_san_pham": r["MaSanPham"], "ten_san_pham": r["TenSanPham"],
                "noi_dung": r["NoiDung"], "so_sao": int(r["SoSao"]), "da_mua_hang": bool(r["DaMuaHang"]),
                "ten_nguoi_danh_gia": r["HoTen"], "diem": round(m["diem"], 4), "nguon": m["nguon"],
                "nhac_khia_canh": noi_khia_canh,
            })
    ung_vien.sort(key=lambda u: (u["nhac_khia_canh"], u["diem"]), reverse=True)
    ung_vien = ung_vien[: s.rag_top_k]
    return {"ung_vien": ung_vien, "buoc": [_buoc(
        "GoiCongCu", t0,
        {"ma_san_pham": st.get("ma_san_pham"), "truy_van": kh["truy_van"], "top_k": s.rag_top_k,
         "embedding": nhung.ten},
        {"so_ket_qua": len(ung_vien),
         "ket_qua": [{"ma_danh_gia": u["ma_danh_gia"], "diem": u["diem"], "nguon": u["nguon"]} for u in ung_vien]},
        "tim_danh_gia")]}


def thong_ke_khia_canh(st: TrangThai) -> dict:
    t0 = time.perf_counter()
    kh = st["ke_hoach"]
    if st.get("ma_san_pham") is None:
        return {"thong_ke": [], "buoc": [_buoc("GoiCongCu", t0, {"ma_san_pham": None},
                                               {"bo_qua": "Câu hỏi không gắn với sản phẩm cụ thể."},
                                               "thong_ke_khia_canh")]}
    rows = truy_van("SELECT TenKhiaCanh, SoTichCuc, SoTrungTinh, SoTieuCuc FROM dbo.vw_ThongKeKhiaCanh "
                    "WHERE MaSanPham = ? ORDER BY MaKhiaCanh", st["ma_san_pham"])
    if kh["khia_canh"]:
        rows = [r for r in rows if r["TenKhiaCanh"] in kh["khia_canh"]]
    thong_ke = [{"khia_canh": r["TenKhiaCanh"], "tich_cuc": r["SoTichCuc"], "trung_tinh": r["SoTrungTinh"],
                 "tieu_cuc": r["SoTieuCuc"]} for r in rows]
    return {"thong_ke": thong_ke, "buoc": [_buoc(
        "GoiCongCu", t0, {"ma_san_pham": st["ma_san_pham"], "khia_canh": kh["khia_canh"]}, thong_ke,
        "thong_ke_khia_canh")]}


def quan_sat(st: TrangThai) -> dict:
    t0 = time.perf_counter()
    ung = st.get("ung_vien", [])
    return {"buoc": [_buoc("QuanSat", t0, None, {
        "so_ung_vien": len(ung),
        "so_nhac_khia_canh": sum(1 for u in ung if u["nhac_khia_canh"]),
        "diem_cao_nhat": max((u["diem"] for u in ung), default=None),
        "co_thong_ke": bool(st.get("thong_ke")),
    })]}


def kiem_soat(st: TrangThai) -> dict:
    """Bắt buộc trích dẫn: chỉ giữ đánh giá đủ liên quan; không đủ → không được tự trả lời."""
    t0 = time.perf_counter()
    kh = st["ke_hoach"]
    if not kh["pham_vi"]:
        return {"che_do": "tu_choi", "dan_chung": [], "buoc": [_buoc(
            "KiemSoat", t0, {"kiem_tra": "pham_vi"}, {"dat": False, "ly_do": "Câu hỏi ngoài phạm vi sản phẩm."})]}

    nguong = _nguong()
    hop_le = [u for u in st.get("ung_vien", []) if u["diem"] >= nguong or u["nguon"] == "khia_canh"]
    if kh["khia_canh"]:
        hop_le = [u for u in hop_le if u["nhac_khia_canh"]]
    dan_chung = [{
        "ma_danh_gia": u["ma_danh_gia"],
        "doan_trich": cau_lien_quan_nhat(u["noi_dung"], st["cau_hoi"], kh["khia_canh"]),
        "diem": round(u["diem"], 2),
        "so_sao": u["so_sao"],
        "da_mua_hang": u["da_mua_hang"],
        "ten_nguoi_danh_gia": u["ten_nguoi_danh_gia"],
        "ten_san_pham": u["ten_san_pham"],
        "noi_dung": u["noi_dung"],
    } for u in hop_le[:4]]
    dat = bool(dan_chung)
    return {"che_do": "tra_loi" if dat else "thieu_dan_chung", "dan_chung": dan_chung, "buoc": [_buoc(
        "KiemSoat", t0, {"kiem_tra": "bat_buoc_trich_dan", "nguong": nguong, "khia_canh": kh["khia_canh"]},
        {"dat": dat, "so_dan_chung": len(dan_chung)})]}


def _ten_doi_tuong(st: TrangThai) -> str:
    return f"sản phẩm “{st['ten_san_pham']}”" if st.get("ten_san_pham") else "các sản phẩm"


def _tra_loi_mau(st: TrangThai) -> str:
    kh, dc = st["ke_hoach"], st["dan_chung"]
    ve = f" về {', '.join(k.lower() for k in kh['khia_canh'])}" if kh["khia_canh"] else ""
    dong = [f"Dựa trên {len(dc)} đánh giá đã duyệt liên quan{ve} của {_ten_doi_tuong(st)}:"]
    for t in st.get("thong_ke", []):
        dong.append(f"• {t['khia_canh']}: {t['tich_cuc']} ý kiến tích cực, {t['trung_tinh']} trung tính, "
                    f"{t['tieu_cuc']} tiêu cực.")
    for i, d in enumerate(dc, 1):
        dong.append(f"• “{d['doan_trich']}” [{i}]")
    if len(dc) < 3:
        dong.append("Số đánh giá còn ít nên chưa đủ để kết luận chắc chắn.")
    return "\n".join(dong)


def _tra_loi_llm(st: TrangThai) -> str | None:
    from app.services.absa.llm import goi_claude, tao_client

    try:
        client = tao_client()
    except MoHinhKhongSanSang:
        return None
    if not (get_settings().anthropic_api_key or client.api_key or getattr(client, "auth_token", None)):
        return None

    dc = st["dan_chung"]
    tai_lieu = "\n".join(f"[{i}] ({d['so_sao']}★{', đã mua hàng' if d['da_mua_hang'] else ''}) {d['noi_dung']}"
                         for i, d in enumerate(dc, 1))
    thong_ke = "\n".join(f"- {t['khia_canh']}: {t['tich_cuc']} tích cực / {t['trung_tinh']} trung tính / "
                         f"{t['tieu_cuc']} tiêu cực" for t in st.get("thong_ke", [])) or "(không có)"
    system = (
        "Bạn là trợ lý tư vấn mỹ phẩm của cửa hàng. Chỉ trả lời dựa trên các đánh giá khách hàng được cung cấp "
        "trong thẻ <danh_gia>; không dùng kiến thức bên ngoài, không bịa thông tin. Mỗi nhận định phải kèm trích "
        "dẫn dạng [n] theo số thứ tự đánh giá. Nếu các đánh giá trái chiều, nêu cả hai phía. Nếu có ít hơn 3 đánh "
        "giá, nói rõ số lượng còn ít. Trả lời bằng tiếng Việt, tối đa khoảng 120 từ, văn bản thường (không markdown)."
    )
    noi_dung = (f"Đối tượng: {_ten_doi_tuong(st)}\nCâu hỏi: {st['cau_hoi']}\n\n"
                f"<thong_ke_khia_canh>\n{thong_ke}\n</thong_ke_khia_canh>\n\n<danh_gia>\n{tai_lieu}\n</danh_gia>")
    try:
        text = goi_claude(client, system=system, noi_dung=noi_dung,
                          effort=get_settings().llm_effort_tra_loi, max_tokens=16000).strip()
    except Exception as e:  # noqa: BLE001 – lỗi API → dùng câu trả lời mẫu
        logger.warning("Claude lỗi khi sinh câu trả lời RAG: %s", e)
        return None
    trich = {int(n) for n in re.findall(r"\[(\d+)\]", text)}
    # Kiểm soát sau sinh: phải có trích dẫn và mọi trích dẫn phải trỏ tới đánh giá có thật.
    if not trich or not trich <= set(range(1, len(dc) + 1)):
        logger.warning("Câu trả lời LLM thiếu/sai trích dẫn %s – dùng câu trả lời mẫu.", trich)
        return None
    return text


def tra_loi(st: TrangThai) -> dict:
    t0 = time.perf_counter()
    che_do_sinh = "mau"
    if st["che_do"] == "tu_choi":
        text = ("Xin lỗi, mình chỉ có thể trả lời các câu hỏi về sản phẩm mỹ phẩm dựa trên đánh giá của khách hàng "
                "(ví dụ: độ thấm, mùi hương, kích ứng, hiệu quả, giá, bao bì). Bạn hãy đặt câu hỏi về sản phẩm nhé!")
    elif st["che_do"] == "thieu_dan_chung":
        kh = st["ke_hoach"]
        ve = f" nhắc tới {', '.join(k.lower() for k in kh['khia_canh'])}" if kh["khia_canh"] else " liên quan tới câu hỏi"
        text = (f"Hiện chưa có đánh giá đã duyệt nào{ve} của {_ten_doi_tuong(st)}, nên mình chưa thể trả lời "
                "có dẫn chứng. Bạn có thể tham khảo mô tả sản phẩm hoặc quay lại khi có thêm đánh giá.")
    else:
        text = _tra_loi_llm(st)
        if text:
            che_do_sinh = "llm"
        else:
            text = _tra_loi_mau(st)
    return {"tra_loi": text, "che_do_sinh": che_do_sinh, "buoc": [_buoc(
        "TraLoi", t0, {"che_do": st["che_do"]},
        {"so_trich_dan": len(st.get("dan_chung", [])), "sinh_bang": che_do_sinh})]}


def _sau_ke_hoach(st: TrangThai) -> str:
    return "tim_danh_gia" if st["ke_hoach"]["pham_vi"] else "kiem_soat"


def _xay_do_thi():
    g = StateGraph(TrangThai)
    for ten, fn in [("lap_ke_hoach", lap_ke_hoach), ("tim_danh_gia", tim_danh_gia),
                    ("thong_ke_khia_canh", thong_ke_khia_canh), ("quan_sat", quan_sat),
                    ("kiem_soat", kiem_soat), ("tra_loi", tra_loi)]:
        g.add_node(ten, fn)
    g.add_edge(START, "lap_ke_hoach")
    g.add_conditional_edges("lap_ke_hoach", _sau_ke_hoach, {"tim_danh_gia": "tim_danh_gia", "kiem_soat": "kiem_soat"})
    g.add_edge("tim_danh_gia", "thong_ke_khia_canh")
    g.add_edge("thong_ke_khia_canh", "quan_sat")
    g.add_edge("quan_sat", "kiem_soat")
    g.add_edge("kiem_soat", "tra_loi")
    g.add_edge("tra_loi", END)
    return g.compile()


_DO_THI = _xay_do_thi()


# ------------------------------------------------------------------ lưu trữ

def _them_tin_nhan(cur, ma_phien: str, ma_nguoi_dung: int | None, ma_san_pham: int | None, nguoi_gui: str,
                   noi_dung: str, dan_chung: list | None = None, do_tre_ms: int | None = None) -> int:
    cur.execute(
        "INSERT dbo.TinNhan (MaPhien, MaNguoiDung, MaSanPham, NguoiGui, NoiDung, DanChung, DoTreMs) "
        "OUTPUT INSERTED.MaTinNhan VALUES (?, ?, ?, ?, ?, ?, ?)",
        ma_phien, ma_nguoi_dung, ma_san_pham, nguoi_gui, noi_dung,
        json.dumps(dan_chung, ensure_ascii=False) if dan_chung is not None else None, do_tre_ms,
    )
    return int(cur.fetchone()[0])


def _json(x: Any) -> str | None:
    return None if x is None else json.dumps(x, ensure_ascii=False, default=str)


class KhongTimThaySanPham(LookupError):
    pass


def hoi(cau_hoi: str, ma_san_pham: int | None, ma_nguoi_dung: int | None, ma_phien: str | None) -> dict:
    t0 = time.perf_counter()
    ma_phien = ma_phien or str(uuid.uuid4())
    ten_san_pham = None
    if ma_san_pham is not None:
        sp = truy_van_mot("SELECT Ten FROM dbo.SanPham WHERE MaSanPham = ?", ma_san_pham)
        if sp is None:
            raise KhongTimThaySanPham(f"Không tìm thấy sản phẩm #{ma_san_pham}.")
        ten_san_pham = sp["Ten"]

    with get_connection() as conn:
        cur = conn.cursor()
        ma_tin_hoi = _them_tin_nhan(cur, ma_phien, ma_nguoi_dung, ma_san_pham, "NguoiDung", cau_hoi)
        conn.commit()

    khia_canh_hop_le = [r["Ten"] for r in truy_van("SELECT Ten FROM dbo.KhiaCanh ORDER BY MaKhiaCanh")]
    try:
        kq: TrangThai = _DO_THI.invoke({"cau_hoi": cau_hoi, "ma_san_pham": ma_san_pham, "ten_san_pham": ten_san_pham,
                                        "khia_canh_hop_le": khia_canh_hop_le, "buoc": []})
        buoc, text, dan_chung = kq["buoc"], kq["tra_loi"], kq.get("dan_chung", [])
        che_do, che_do_sinh = kq["che_do"], kq["che_do_sinh"]
    except Exception as e:  # noqa: BLE001 – vẫn lưu tin nhắn + bước Loi để truy vết
        logger.exception("Tác tử lỗi khi trả lời câu hỏi.")
        buoc = [_buoc("Loi", t0, {"cau_hoi": cau_hoi}, {"loi": str(e)[:500]})]
        text = "Xin lỗi, trợ lý đang gặp sự cố khi tra cứu đánh giá. Bạn vui lòng thử lại sau."
        dan_chung, che_do, che_do_sinh = [], "loi", "mau"

    do_tre = int((time.perf_counter() - t0) * 1000)
    dan_chung_luu = [{"maDanhGia": d["ma_danh_gia"], "doanTrich": d["doan_trich"], "diem": d["diem"]} for d in dan_chung]
    with get_connection() as conn:
        cur = conn.cursor()
        ma_tin_tra_loi = _them_tin_nhan(cur, ma_phien, ma_nguoi_dung, ma_san_pham, "TroLy", text,
                                        dan_chung_luu, do_tre)
        for i, b in enumerate(buoc, 1):
            cur.execute(
                "INSERT dbo.NhatKyTacTu (MaTinNhan, BuocSo, LoaiBuoc, TenCongCu, DauVao, DauRa, DoTreMs) "
                "VALUES (?, ?, ?, ?, ?, ?, ?)",
                ma_tin_tra_loi, i, b["loai_buoc"], b["ten_cong_cu"], _json(b["dau_vao"]), _json(b["dau_ra"]),
                b["do_tre_ms"],
            )
        conn.commit()

    return {
        "ma_phien": ma_phien,
        "ma_tin_nhan_hoi": ma_tin_hoi,
        "ma_tin_nhan": ma_tin_tra_loi,
        "tra_loi": text,
        "che_do": che_do,
        "sinh_bang": che_do_sinh,
        "dan_chung": [{k: d[k] for k in ("ma_danh_gia", "doan_trich", "diem", "so_sao", "da_mua_hang",
                                         "ten_nguoi_danh_gia", "ten_san_pham")} for d in dan_chung],
        "buoc": [{"buoc_so": i, **b} for i, b in enumerate(buoc, 1)],
        "do_tre_ms": do_tre,
    }
