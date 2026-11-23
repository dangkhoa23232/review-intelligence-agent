"""Chạy thực nghiệm ABSA và xuất báo cáo từ dòng lệnh (không cần mở trang quản trị).

Chạy trong thư mục ai-service (để đọc .env):

    $env:PYTHONUTF8 = "1"
    .venv\\Scripts\\python -m scripts.thuc_nghiem kiem-tra              # cấu hình nào chạy được
    .venv\\Scripts\\python -m scripts.thuc_nghiem kiem-tra --goi-thu    # + gọi Claude 1 lần để xác thực khóa
    .venv\\Scripts\\python -m scripts.thuc_nghiem nap-mau
    .venv\\Scripts\\python -m scripts.thuc_nghiem chay PhoBERT LLM_KhongVD LLM_ItVD
    .venv\\Scripts\\python -m scripts.thuc_nghiem bao-cao --ra ..\\docs\\thuc-nghiem\\ket-qua.md

Luồng thực nghiệm KHÔNG mở Qdrant → chạy được song song với tiến trình ai-service đang chạy
(nhưng đừng bấm "Chạy" trên trang quản trị cùng lúc với lệnh `chay` này).
Kết quả ghi dbo.KetQuaThucNghiem (MERGE theo MaMau + MoHinh); chỉ số đọc từ dbo.vw_ChiSoThucNghiem.
"""

from __future__ import annotations

import argparse
import json
import logging
import statistics
import sys
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path

from app.db.sqlserver import truy_van
from app.services.absa import doc_khia_canh, lay_bo_phan_tich, trang_thai_mo_hinh
from app.services.absa.base import MO_HINH, MoHinhKhongSanSang
from app.services import thuc_nghiem

NHAN = ("TichCuc", "TrungTinh", "TieuCuc")
TEN_NHAN = {"TichCuc": "Tích cực", "TrungTinh": "Trung tính", "TieuCuc": "Tiêu cực"}
TEN_MO_HINH = {"PhoBERT": "PhoBERT (baseline)", "LLM_KhongVD": "LLM zero-shot", "LLM_ItVD": "LLM few-shot"}


# ====================================================================== lệnh

def lenh_kiem_tra(goi_thu: bool) -> int:
    tt = trang_thai_mo_hinh(thu_khoi_tao=True)
    for m, s in tt.items():
        print(f"{m:12} {'SẴN SÀNG' if s['san_sang'] else 'KHÔNG'}  {s['ly_do'] or ''}")
    if goi_thu:
        for m in ("LLM_KhongVD",):
            if not tt[m]["san_sang"]:
                print(f"Bỏ qua gọi thử {m}: chưa sẵn sàng.")
                continue
            try:
                kq = lay_bo_phan_tich(m).phan_tich("Kem thấm nhanh, mùi dễ chịu nhưng giá hơi cao.", doc_khia_canh())
                print(f"Gọi thử {m} OK → {kq.cam_xuc}, khía cạnh: {[(k.ten, k.cam_xuc) for k in kq.khia_canh]}")
            except Exception as e:  # noqa: BLE001
                print(f"Gọi thử {m} LỖI: {e}")
                return 1
    return 0


def lenh_nap_mau() -> int:
    kq = thuc_nghiem.nap_mau()
    print(f"Đã thêm {kq['da_them']} mẫu, bỏ qua {kq['bo_qua_trung']} mẫu trùng.")
    for r in kq["thong_ke"]:
        print(f"  {r['TapDuLieu']:8} {r['CamXucChuan']:10} {r['SoMau']}")
    return 0


def lenh_chay(cac_mo_hinh: list[str]) -> int:
    sai = [m for m in cac_mo_hinh if m not in MO_HINH]
    if sai:
        print(f"Mô hình không hợp lệ: {sai}. Chấp nhận: {list(MO_HINH)}")
        return 2
    for m in cac_mo_hinh:
        try:
            lay_bo_phan_tich(m)
        except MoHinhKhongSanSang as e:
            print(f"⚠ {m} không chạy được: {e}")
    thuc_nghiem.chay(cac_mo_hinh)
    tt = thuc_nghiem.trang_thai()
    for m, p in tt["tien_do"].items():
        print(f"{m}: {p['da_xong']}/{p['tong']} mẫu")
    for m, ly_do in tt.get("bo_qua", {}).items():
        print(f"Bỏ qua {m}: {ly_do}")
    for m, l in tt["loi"].items():
        print(f"Lỗi gần nhất {m}: {l}")
    return 0


# ====================================================================== báo cáo

def _doc_json(s: str | None) -> dict:
    try:
        d = json.loads(s) if s else {}
        return d if isinstance(d, dict) else {}
    except json.JSONDecodeError:
        return {}


def nhom_chinh(nhom_loi: str) -> str:
    """Gộp NhomLoi chi tiết thành nhóm chính để thống kê."""
    if " – " in nhom_loi:
        return nhom_loi.split(" – ")[0]
    if nhom_loi.startswith("Đoán trung tính"):
        return "Đoán trung tính cho câu có cảm xúc"
    if nhom_loi.startswith("Đảo cực"):
        return "Đảo cực cảm xúc (Tích cực ↔ Tiêu cực)"
    return nhom_loi


def _pt(x: float | None) -> str:
    return "–" if x is None else f"{x * 100:.1f}%"


def lenh_bao_cao(duong_dan_ra: Path | None) -> int:
    bo_mau = truy_van("SELECT TapDuLieu, CamXucChuan, COUNT(*) AS SoMau FROM dbo.MauKiemThu GROUP BY TapDuLieu, CamXucChuan")
    so_kiem_thu = sum(r["SoMau"] for r in bo_mau if r["TapDuLieu"] == "KiemThu")
    so_vi_du = sum(r["SoMau"] for r in bo_mau if r["TapDuLieu"] == "ViDuMau")
    phan_bo = {r["CamXucChuan"]: r["SoMau"] for r in bo_mau if r["TapDuLieu"] == "KiemThu"}

    view = {r["MoHinh"]: r for r in truy_van("SELECT * FROM dbo.vw_ChiSoThucNghiem")}
    dong = truy_van(
        """SELECT kq.MaMau, kq.MoHinh, kq.CamXucDuDoan, kq.KhiaCanhDuDoan, kq.LaDung, kq.NhomLoi, kq.DoTreMs, kq.NgayChay,
                  m.NoiDung, m.CamXucChuan, m.KhiaCanhChuan
           FROM dbo.KetQuaThucNghiem kq JOIN dbo.MauKiemThu m ON m.MaMau = kq.MaMau AND m.TapDuLieu = 'KiemThu'
           ORDER BY kq.MoHinh, kq.MaMau""")
    theo_mh: dict[str, list[dict]] = defaultdict(list)
    for d in dong:
        theo_mh[d["MoHinh"]].append(d)

    L: list[str] = []
    gio = datetime.now(timezone.utc).astimezone().strftime("%d/%m/%Y %H:%M")
    L += [f"# Kết quả thực nghiệm ABSA", "",
          f"_Sinh tự động bởi `scripts/thuc_nghiem.py bao-cao` lúc {gio}. Chỉ số chính đọc từ `dbo.vw_ChiSoThucNghiem`._", "",
          "## Bộ dữ liệu", "",
          f"- Tập kiểm thử (`MauKiemThu.TapDuLieu = 'KiemThu'`): **{so_kiem_thu} mẫu** – "
          + ", ".join(f"{TEN_NHAN[k]} {phan_bo.get(k, 0)}" for k in NHAN) + ".",
          f"- Ví dụ few-shot (`TapDuLieu = 'ViDuMau'`): {so_vi_du} mẫu – tách riêng, không trùng tập kiểm thử.", ""]

    # ---- bảng chính
    L += ["## Chỉ số theo cấu hình (cảm xúc tổng thể)", "",
          "| Cấu hình | Số mẫu | Accuracy | Precision (macro) | Recall (macro) | F1 (macro) | Độ trễ TB | Trung vị | P95 | F1 khía cạnh (micro) |",
          "|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|"]
    canh_bao = []
    for mh in MO_HINH:
        v = view.get(mh)
        ds = theo_mh.get(mh, [])
        if not v:
            L.append(f"| {TEN_MO_HINH[mh]} | 0 | _chưa chạy_ | | | | | | | |")
            continue
        if v["SoMau"] < so_kiem_thu:
            # Chưa chạy đủ bộ kiểm thử (vd. chỉ có dòng minh họa của schema) → không in số liệu dễ gây hiểu nhầm.
            L.append(f"| {TEN_MO_HINH[mh]} | {v['SoMau']}/{so_kiem_thu} | _chưa chạy đủ_ | | | | | | | |")
            canh_bao.append(f"- ⚠ **{TEN_MO_HINH[mh]}** mới có {v['SoMau']}/{so_kiem_thu} mẫu (dữ liệu minh họa / lượt chạy dở) – "
                            f"chưa có số liệu so sánh. Chạy: `python -m scripts.thuc_nghiem chay {mh}` (cần ANTHROPIC_API_KEY).")
            continue
        tre = sorted(d["DoTreMs"] for d in ds if d["DoTreMs"] is not None)
        trung_vi = statistics.median(tre) if tre else None
        p95 = tre[min(len(tre) - 1, int(round(0.95 * (len(tre) - 1))))] if tre else None
        f1_kc = _f1_khia_canh(ds)
        L.append(f"| {TEN_MO_HINH[mh]} | {v['SoMau']} | {_pt(v['DoChinhXac'])} | {_pt(v['DoChuanXacMacro'])} | "
                 f"{_pt(v['DoPhuMacro'])} | {_pt(v['DiemF1Macro'])} | "
                 f"{'–' if v['DoTreTrungBinhMs'] is None else f'{v['DoTreTrungBinhMs']:.0f} ms'} | "
                 f"{'–' if trung_vi is None else f'{trung_vi:.0f} ms'} | {'–' if p95 is None else f'{p95} ms'} | "
                 f"{_pt(f1_kc)} |")
    L += [""] + canh_bao + ([""] if canh_bao else [])
    L += ["Ghi chú: Precision/Recall/F1 macro = trung bình không trọng số trên 3 nhãn. F1 khía cạnh (micro) đo việc "
          "phát hiện đúng tập khía cạnh được nhắc tới (so `KhiaCanhDuDoan` với `KhiaCanhChuan`).", ""]

    # ---- theo nhãn + ma trận nhầm lẫn
    for mh in MO_HINH:
        ds = theo_mh.get(mh, [])
        if not ds or len(ds) < so_kiem_thu:
            continue
        L += [f"## {TEN_MO_HINH[mh]} (`{mh}`)", "", "### Chỉ số theo nhãn", "",
              "| Nhãn | Precision | Recall | F1 | Số mẫu chuẩn |", "|---|---:|---:|---:|---:|"]
        for n in NHAN:
            tp = sum(1 for d in ds if d["CamXucDuDoan"] == n and d["CamXucChuan"] == n)
            fp = sum(1 for d in ds if d["CamXucDuDoan"] == n and d["CamXucChuan"] != n)
            fn = sum(1 for d in ds if d["CamXucDuDoan"] != n and d["CamXucChuan"] == n)
            p = tp / (tp + fp) if tp + fp else 0.0
            r = tp / (tp + fn) if tp + fn else 0.0
            f = 2 * p * r / (p + r) if p + r else 0.0
            L.append(f"| {TEN_NHAN[n]} | {_pt(p)} | {_pt(r)} | {_pt(f)} | {tp + fn} |")
        L += ["", "### Ma trận nhầm lẫn (hàng = nhãn chuẩn, cột = dự đoán)", "",
              "| Chuẩn \\ Dự đoán | " + " | ".join(TEN_NHAN[n] for n in NHAN) + " |", "|---|---:|---:|---:|"]
        dem = Counter((d["CamXucChuan"], d["CamXucDuDoan"]) for d in ds)
        for c in NHAN:
            L.append(f"| {TEN_NHAN[c]} | " + " | ".join(
                (f"**{dem[(c, p)]}**" if c == p else str(dem[(c, p)])) for p in NHAN) + " |")

        sai = [d for d in ds if not d["LaDung"]]
        L += ["", f"### Phân tích lỗi – {len(sai)}/{len(ds)} mẫu sai", ""]
        if sai:
            nhom = defaultdict(list)
            for d in sai:
                nhom[nhom_chinh(d["NhomLoi"] or "Không phân loại")].append(d)
            L += ["| Nhóm lỗi | Số mẫu | Tỉ lệ trong mẫu sai | Chi tiết |", "|---|---:|---:|---|"]
            for ten, ds_nhom in sorted(nhom.items(), key=lambda x: -len(x[1])):
                chi_tiet = Counter(d["NhomLoi"] for d in ds_nhom)
                L.append(f"| {ten} | {len(ds_nhom)} | {_pt(len(ds_nhom) / len(sai))} | "
                         + "; ".join(f"{k} ({v})" for k, v in chi_tiet.most_common()) + " |")
            L += ["", "Ví dụ mẫu sai:", ""]
            for d in sai[:8]:
                L.append(f"- #{d['MaMau']} – “{d['NoiDung']}” → chuẩn **{TEN_NHAN[d['CamXucChuan']]}**, "
                         f"dự đoán **{TEN_NHAN[d['CamXucDuDoan']]}** ({d['NhomLoi']})")
        L.append("")

    L += ["## Hạn chế & lưu ý khi đưa vào báo cáo", "",
          "- Bộ mẫu kiểm thử là dữ liệu **tổng hợp** (`ai-service/data/mau_kiem_thu.json`), gán nhãn thủ công; quy mô nhỏ "
          f"({so_kiem_thu} mẫu) nên mỗi mẫu ≈ {100 / max(1, so_kiem_thu):.1f} điểm phần trăm accuracy.",
          "- Từ điển khía cạnh của PhoBERT được xây cùng thời điểm với bộ mẫu → F1 khía cạnh của PhoBERT là **ước lượng lạc quan**. "
          "Nên bổ sung mẫu từ đánh giá thật do người khác gán nhãn.",
          "- Độ trễ PhoBERT đo trên GPU cục bộ; độ trễ LLM gồm thời gian mạng tới Claude API.",
          "- Chỉ các cấu hình đã chạy đủ bộ kiểm thử mới được in số liệu; cấu hình thiếu mẫu (vd. vài dòng minh họa có sẵn "
          "trong schema) được đánh dấu “chưa chạy đủ”.",
          "- PhoBERT được chạy khởi động 1 lần (không tính giờ) trước khi đo độ trễ.", ""]

    van_ban = "\n".join(L)
    if duong_dan_ra:
        duong_dan_ra.parent.mkdir(parents=True, exist_ok=True)
        duong_dan_ra.write_text(van_ban, encoding="utf-8")
        print(f"Đã ghi báo cáo: {duong_dan_ra.resolve()}")
    else:
        print(van_ban)
    return 0


def _f1_khia_canh(ds: list[dict]) -> float | None:
    tp = fp = fn = 0
    for d in ds:
        chuan, du_doan = set(_doc_json(d["KhiaCanhChuan"])), set(_doc_json(d["KhiaCanhDuDoan"]))
        tp += len(chuan & du_doan)
        fp += len(du_doan - chuan)
        fn += len(chuan - du_doan)
    if not ds:
        return None
    p = tp / (tp + fp) if tp + fp else 0.0
    r = tp / (tp + fn) if tp + fn else 0.0
    return 2 * p * r / (p + r) if p + r else 0.0


# ====================================================================== main

def main(argv: list[str] | None = None) -> int:
    logging.basicConfig(level=logging.WARNING, format="%(levelname)s %(name)s: %(message)s")
    ap = argparse.ArgumentParser(prog="python -m scripts.thuc_nghiem", description=__doc__.split("\n")[0])
    sub = ap.add_subparsers(dest="lenh", required=True)
    kt = sub.add_parser("kiem-tra", help="Cấu hình nào chạy được trên máy")
    kt.add_argument("--goi-thu", action="store_true", help="Gọi Claude 1 lần để xác thực khóa (tốn rất ít token)")
    sub.add_parser("nap-mau", help="Nạp data/mau_kiem_thu.json vào MauKiemThu")
    ch = sub.add_parser("chay", help="Chạy thực nghiệm trên toàn bộ mẫu KiemThu")
    ch.add_argument("mo_hinh", nargs="*", default=list(MO_HINH), help="PhoBERT LLM_KhongVD LLM_ItVD (mặc định: cả 3)")
    bc = sub.add_parser("bao-cao", help="Xuất báo cáo Markdown")
    bc.add_argument("--ra", type=Path, help="Ghi ra file (mặc định in ra màn hình)")
    a = ap.parse_args(argv)

    if a.lenh == "kiem-tra":
        return lenh_kiem_tra(a.goi_thu)
    if a.lenh == "nap-mau":
        return lenh_nap_mau()
    if a.lenh == "chay":
        return lenh_chay(a.mo_hinh)
    return lenh_bao_cao(a.ra)


if __name__ == "__main__":
    sys.exit(main())
