"""Tính lại dbo.TomTatKhiaCanh của một sản phẩm từ KetQuaKhiaCanh của các đánh giá DaDuyet.

Tóm tắt trích xuất (extractive): ưu/nhược điểm là các cụm từ dẫn chứng xuất hiện nhiều nhất,
vấn đề lặp lại là ý tiêu cực được ≥ 2 đánh giá nhắc tới; DanChung là danh sách MaDanhGia.
"""

import json
import logging
from collections import Counter, defaultdict

from app.db.sqlserver import get_connection, truy_van

logger = logging.getLogger(__name__)

SO_Y_TOI_DA = 3


def _gop_cum_tu(ds: list[str], toi_da: int) -> str | None:
    if not ds:
        return None
    dem = Counter(c.strip().rstrip(".").lower() for c in ds if c and c.strip())
    goc = {c.strip().rstrip(".").lower(): c.strip().rstrip(".") for c in ds if c and c.strip()}
    y = [goc[k][:1].upper() + goc[k][1:] for k, _ in dem.most_common(SO_Y_TOI_DA)]
    return "; ".join(y)[:toi_da] or None


def cap_nhat_tom_tat(ma_san_pham: int) -> int:
    rows = truy_van(
        """SELECT x.MaKhiaCanh, k.Ten, x.CamXuc, x.CumTuDanChung, x.MaDanhGia
           FROM dbo.KetQuaKhiaCanh x
           JOIN dbo.DanhGia dg ON dg.MaDanhGia = x.MaDanhGia AND dg.TrangThai = 'DaDuyet'
           JOIN dbo.KhiaCanh k ON k.MaKhiaCanh = x.MaKhiaCanh
           WHERE dg.MaSanPham = ?""",
        ma_san_pham,
    )
    theo_khia_canh: dict[int, list[dict]] = defaultdict(list)
    for r in rows:
        theo_khia_canh[r["MaKhiaCanh"]].append(r)

    with get_connection() as conn:
        cur = conn.cursor()
        for ma_kc, ds in theo_khia_canh.items():
            ten = ds[0]["Ten"]
            tich = [r for r in ds if r["CamXuc"] == "TichCuc"]
            tieu = [r for r in ds if r["CamXuc"] == "TieuCuc"]

            uu = _gop_cum_tu([r["CumTuDanChung"] for r in tich], 1000)
            nhuoc = _gop_cum_tu([r["CumTuDanChung"] for r in tieu], 1000)
            van_de = None
            if len(tieu) >= 2:
                van_de = f"{len(tieu)} đánh giá phản ánh tiêu cực về {ten.lower()}"
                if nhuoc:
                    van_de += f": {nhuoc.split('; ')[0].lower()}"
                van_de = van_de[:500]
            # Dẫn chứng: đánh giá tiêu cực trước (cần lưu ý), rồi tích cực; tối đa 10.
            dan_chung = list(dict.fromkeys([r["MaDanhGia"] for r in tieu] + [r["MaDanhGia"] for r in tich]))[:10]

            cur.execute(
                """MERGE dbo.TomTatKhiaCanh AS t
                   USING (SELECT ? AS MaSanPham, ? AS MaKhiaCanh) AS s
                     ON t.MaSanPham = s.MaSanPham AND t.MaKhiaCanh = s.MaKhiaCanh
                   WHEN MATCHED THEN UPDATE SET SoTichCuc = ?, SoTieuCuc = ?, UuDiem = ?, NhuocDiem = ?,
                        VanDeLapLai = ?, DanChung = ?, NgayCapNhat = SYSUTCDATETIME()
                   WHEN NOT MATCHED THEN INSERT (MaSanPham, MaKhiaCanh, SoTichCuc, SoTieuCuc, UuDiem, NhuocDiem,
                        VanDeLapLai, DanChung) VALUES (s.MaSanPham, s.MaKhiaCanh, ?, ?, ?, ?, ?, ?);""",
                ma_san_pham, ma_kc,
                len(tich), len(tieu), uu, nhuoc, van_de, json.dumps(dan_chung),
                len(tich), len(tieu), uu, nhuoc, van_de, json.dumps(dan_chung),
            )

        # Khía cạnh không còn đánh giá đã duyệt nào nhắc tới → xóa dòng tóm tắt.
        if theo_khia_canh:
            giu = ",".join("?" * len(theo_khia_canh))
            cur.execute(f"DELETE FROM dbo.TomTatKhiaCanh WHERE MaSanPham = ? AND MaKhiaCanh NOT IN ({giu})",
                        ma_san_pham, *theo_khia_canh.keys())
        else:
            cur.execute("DELETE FROM dbo.TomTatKhiaCanh WHERE MaSanPham = ?", ma_san_pham)
        conn.commit()

    logger.info("Cập nhật TomTatKhiaCanh SanPham#%s: %s khía cạnh.", ma_san_pham, len(theo_khia_canh))
    return len(theo_khia_canh)
