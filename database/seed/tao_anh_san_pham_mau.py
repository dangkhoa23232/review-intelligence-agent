"""Vẽ ảnh minh họa sản phẩm (WebP 900x900) cho dữ liệu mẫu.

Ảnh minh họa phẳng (không phải ảnh chụp/logo thật) – thay bằng ảnh thật qua trang /admin/san-pham.
Chạy: python tao_anh.py <thu_muc_dich>
"""
import math
import random
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageFont

S = 2            # supersample
W = H = 900
FONT = "C:/Windows/Fonts/"


def f(ten, co):
    return ImageFont.truetype(FONT + ten, co * S)


def hex2rgb(h, a=255):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4)) + (a,)


def tron(c1, c2, t):
    return tuple(int(c1[i] + (c2[i] - c1[i]) * t) for i in range(4))


def nen(mau_nen, seed):
    """Nền gradient dọc + vài vòng tròn mờ."""
    w, h = W * S, H * S
    img = Image.new("RGBA", (w, h))
    tren, duoi = hex2rgb(mau_nen[0]), hex2rgb(mau_nen[1])
    d = ImageDraw.Draw(img)
    for y in range(h):
        d.line([(0, y), (w, y)], fill=tron(tren, duoi, y / h))
    rnd = random.Random(seed)
    lop = Image.new("RGBA", (w, h), (255, 255, 255, 0))
    dl = ImageDraw.Draw(lop)
    for _ in range(5):
        r = rnd.randint(120, 320) * S
        x, y = rnd.randint(0, w), rnd.randint(0, h)
        dl.ellipse([x - r, y - r, x + r, y + r], fill=(255, 255, 255, 70))
    lop = lop.filter(ImageFilter.GaussianBlur(40 * S))
    return Image.alpha_composite(img, lop)


def bong(img, hop):
    """Bóng đổ elip dưới đáy sản phẩm."""
    x0, x1, y = hop
    lop = Image.new("RGBA", img.size, (40, 30, 50, 0))
    ImageDraw.Draw(lop).ellipse([x0, y - 22 * S, x1, y + 22 * S], fill=(40, 30, 50, 90))  # bóng
    return Image.alpha_composite(img, lop.filter(ImageFilter.GaussianBlur(18 * S)))


def anh_sang(img, hop, ban_kinh):
    """Vệt sáng dọc bên trái thân chai."""
    x0, y0, x1, y1 = hop
    lop = Image.new("RGBA", img.size, (255, 255, 255, 0))
    rong = (x1 - x0)
    ImageDraw.Draw(lop).rounded_rectangle(
        [x0 + rong * 0.12, y0 + 30 * S, x0 + rong * 0.22, y1 - 40 * S], radius=ban_kinh, fill=(255, 255, 255, 60))
    return Image.alpha_composite(img, lop.filter(ImageFilter.GaussianBlur(8 * S)))


def nhan(d, cx, y, sp, rong_toi_da, mau_chu):
    """Chữ trên nhãn: thương hiệu, dòng sản phẩm (tự xuống dòng), dung tích."""
    ft = f("segoeuib.ttf", sp.get("co_thuong_hieu", 50))
    d.text((cx, y), sp["thuong_hieu"], font=ft, fill=mau_chu, anchor="mt")
    y += ft.size + 18 * S
    fd = f("segoeui.ttf", 30)
    dong, hien = [], ""
    for tu in sp["dong"].split():
        thu = (hien + " " + tu).strip()
        if d.textlength(thu, font=fd) > rong_toi_da and hien:
            dong.append(hien)
            hien = tu
        else:
            hien = thu
    dong.append(hien)
    for x in dong:
        d.text((cx, y), x, font=fd, fill=mau_chu, anchor="mt")
        y += fd.size + 8 * S
    y += 10 * S
    d.line([(cx - 40 * S, y), (cx + 40 * S, y)], fill=mau_chu, width=2 * S)
    y += 18 * S
    d.text((cx, y), sp["dung_tich"], font=f("segoeuisl.ttf", 28), fill=mau_chu, anchor="mt")


def ve_tuyp(img, sp):
    """Tuýp đứng trên nắp."""
    d = ImageDraw.Draw(img)
    cx = W * S // 2
    rong, cao = 300 * S, 560 * S
    y_day = 790 * S
    x0, x1 = cx - rong // 2, cx + rong // 2
    y_nap = y_day - 90 * S
    img = bong(img, (x0 - 10 * S, x1 + 10 * S, y_day))
    d = ImageDraw.Draw(img)
    # nắp
    d.rounded_rectangle([cx - 110 * S, y_nap, cx + 110 * S, y_day], radius=22 * S, fill=hex2rgb(sp["mau_nap"]))
    # thân: hình thang (đầu dẹt rộng hơn), đáy bo tròn
    y0 = y_nap - cao
    than = [(x0 - 20 * S, y0), (x1 + 20 * S, y0), (x1, y_nap - 40 * S), (cx + 110 * S, y_nap + 4 * S),
            (cx - 110 * S, y_nap + 4 * S), (x0, y_nap - 40 * S)]
    d.polygon(than, fill=hex2rgb(sp["mau_than"]))
    # đường ép đầu tuýp
    d.rectangle([x0 - 20 * S, y0, x1 + 20 * S, y0 + 26 * S], fill=hex2rgb(sp["mau_nap"]))
    for i in range(1, 12):
        x = x0 - 20 * S + i * (rong + 40 * S) / 12
        d.line([(x, y0 + 4 * S), (x, y0 + 22 * S)], fill=hex2rgb("#ffffff", 80), width=2 * S)
    # dải nhãn
    d.rectangle([x0 + 10 * S, y0 + 120 * S, x1 - 10 * S, y0 + 150 * S], fill=hex2rgb(sp["mau_nhan"]))
    nhan(d, cx, y0 + 190 * S, sp, rong - 50 * S, hex2rgb(sp["mau_chu"]))
    return anh_sang(img, (x0, y0 + 30 * S, x1, y_nap - 60 * S), 20 * S)


def ve_chai_bom(img, sp):
    """Chai có vòi bơm (dầu gội / dầu xả)."""
    cx = W * S // 2
    rong, y_day = 290 * S, 820 * S
    x0, x1 = cx - rong // 2, cx + rong // 2
    y0 = 300 * S
    img = bong(img, (x0, x1, y_day))
    d = ImageDraw.Draw(img)
    # vòi bơm
    d.rectangle([cx - 14 * S, 160 * S, cx + 14 * S, 250 * S], fill=hex2rgb(sp["mau_nap"]))
    d.rounded_rectangle([cx - 40 * S, 140 * S, cx + 120 * S, 180 * S], radius=14 * S, fill=hex2rgb(sp["mau_nap"]))
    d.rounded_rectangle([cx - 70 * S, 240 * S, cx + 70 * S, y0 + 20 * S], radius=16 * S, fill=hex2rgb(sp["mau_nap"]))
    # thân
    d.rounded_rectangle([x0, y0, x1, y_day], radius=70 * S, fill=hex2rgb(sp["mau_than"]))
    # nhãn
    d.rounded_rectangle([x0 + 26 * S, y0 + 120 * S, x1 - 26 * S, y_day - 90 * S], radius=18 * S, fill=hex2rgb(sp["mau_nhan"]))
    nhan(d, cx, y0 + 160 * S, sp, rong - 90 * S, hex2rgb(sp["mau_chu"]))
    return anh_sang(img, (x0, y0, x1, y_day), 30 * S)


def ve_hu(img, sp):
    """Hũ kem / gel."""
    cx = W * S // 2
    rong, y_day = 440 * S, 760 * S
    x0, x1 = cx - rong // 2, cx + rong // 2
    y_nap0, y_nap1 = 330 * S, 450 * S
    img = bong(img, (x0, x1, y_day))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([x0, y_nap1 - 10 * S, x1, y_day], radius=50 * S, fill=hex2rgb(sp["mau_than"]))
    d.rounded_rectangle([x0 - 12 * S, y_nap0, x1 + 12 * S, y_nap1], radius=30 * S, fill=hex2rgb(sp["mau_nap"]))
    for i in range(1, 30):
        x = x0 - 12 * S + i * (rong + 24 * S) / 30
        d.line([(x, y_nap0 + 14 * S), (x, y_nap1 - 14 * S)], fill=hex2rgb("#ffffff", 40), width=2 * S)
    d.rounded_rectangle([x0 + 30 * S, y_nap1 + 30 * S, x1 - 30 * S, y_day - 30 * S], radius=20 * S, fill=hex2rgb(sp["mau_nhan"]))
    sp2 = dict(sp, co_thuong_hieu=44)
    nhan(d, cx, y_nap1 + 46 * S, sp2, rong - 100 * S, hex2rgb(sp["mau_chu"]))
    return anh_sang(img, (x0, y_nap1, x1, y_day), 20 * S)


def ve_chai_nho_giot(img, sp):
    """Chai serum có ống nhỏ giọt."""
    cx = W * S // 2
    rong, y_day = 260 * S, 800 * S
    x0, x1 = cx - rong // 2, cx + rong // 2
    y0 = 380 * S
    img = bong(img, (x0, x1, y_day))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([cx - 40 * S, 150 * S, cx + 40 * S, 290 * S], radius=40 * S, fill=hex2rgb(sp["mau_nap"]))
    d.rounded_rectangle([cx - 70 * S, 280 * S, cx + 70 * S, y0 + 10 * S], radius=12 * S, fill=hex2rgb(sp["mau_nap"]))
    d.rounded_rectangle([x0, y0, x1, y_day], radius=46 * S, fill=hex2rgb(sp["mau_than"]))
    d.rounded_rectangle([x0 + 22 * S, y0 + 70 * S, x1 - 22 * S, y_day - 60 * S], radius=12 * S, fill=hex2rgb(sp["mau_nhan"]))
    nhan(d, cx, y0 + 100 * S, dict(sp, co_thuong_hieu=34), rong - 70 * S, hex2rgb(sp["mau_chu"]))
    return anh_sang(img, (x0, y0, x1, y_day), 20 * S)


def ve_chai_nap_bat(img, sp):
    """Chai bóp nắp bật (gel rửa mặt)."""
    cx = W * S // 2
    rong, y_day = 300 * S, 810 * S
    x0, x1 = cx - rong // 2, cx + rong // 2
    y0 = 290 * S
    img = bong(img, (x0, x1, y_day))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([cx - 90 * S, 190 * S, cx + 90 * S, y0 + 30 * S], radius=26 * S, fill=hex2rgb(sp["mau_nap"]))
    d.rounded_rectangle([x0, y0, x1, y_day], radius=60 * S, fill=hex2rgb(sp["mau_than"]))
    d.rounded_rectangle([x0 + 28 * S, y0 + 110 * S, x1 - 28 * S, y_day - 80 * S], radius=16 * S, fill=hex2rgb(sp["mau_nhan"]))
    nhan(d, cx, y0 + 150 * S, sp, rong - 90 * S, hex2rgb(sp["mau_chu"]))
    return anh_sang(img, (x0, y0, x1, y_day), 26 * S)


def ve_ket_cau(img, sp):
    """Ảnh phụ: vệt kết cấu sản phẩm (bọt / gel / kem) + chú thích."""
    w, h = img.size
    rnd = random.Random(sp["tep"])
    lop = Image.new("RGBA", img.size, (255, 255, 255, 0))
    d = ImageDraw.Draw(lop)
    mau = hex2rgb(sp["mau_ket_cau"])
    kieu = sp["ket_cau"]
    cx, cy = w // 2, int(h * 0.46)
    if kieu == "bot":
        for _ in range(140):
            r = rnd.randint(18, 70) * S
            a, k = rnd.uniform(0, 2 * math.pi), rnd.gauss(0, 150) * S
            x, y = cx + math.cos(a) * k * 1.4, cy + math.sin(a) * k * 0.8
            d.ellipse([x - r, y - r, x + r, y + r], fill=mau, outline=hex2rgb("#ffffff", 230), width=3 * S)
        lop = lop.filter(ImageFilter.GaussianBlur(1 * S))
    else:
        diem = []
        for i in range(36):
            g = 2 * math.pi * i / 36
            r = (250 + 40 * math.sin(3 * g) + rnd.uniform(-15, 15)) * S
            diem.append((cx + math.cos(g) * r * 1.3, cy + math.sin(g) * r * 0.75))
        d.polygon(diem, fill=mau)
        lop = lop.filter(ImageFilter.GaussianBlur(6 * S))
        d = ImageDraw.Draw(lop)
        d.ellipse([cx - 230 * S, cy - 120 * S, cx - 60 * S, cy - 50 * S], fill=(255, 255, 255, 150))
        if kieu == "kem":
            for i in range(4):
                d.arc([cx - (200 - i * 45) * S, cy - (110 - i * 25) * S, cx + (200 - i * 45) * S, cy + (110 - i * 25) * S],
                      start=200, end=340, fill=hex2rgb("#ffffff", 160), width=8 * S)
        lop = lop.filter(ImageFilter.GaussianBlur(2 * S))
    img = Image.alpha_composite(img, lop)
    d = ImageDraw.Draw(img)
    d.text((w // 2, int(h * 0.80)), sp["chu_thich"], font=f("segoeuib.ttf", 40), fill=hex2rgb(sp["mau_chu_nen"]), anchor="mm")
    d.text((w // 2, int(h * 0.86)), f'{sp["thuong_hieu"]} · {sp["dong"]}', font=f("segoeui.ttf", 28),
           fill=hex2rgb(sp["mau_chu_nen"], 200), anchor="mm")
    return img


VE = {"tuyp": ve_tuyp, "chai_bom": ve_chai_bom, "hu": ve_hu, "nho_giot": ve_chai_nho_giot, "nap_bat": ve_chai_nap_bat}

SAN_PHAM = [
    # ---- 4 sản phẩm có sẵn trong schema
    dict(tep="gel-rua-mat-bi-dao", hinh="nap_bat", thuong_hieu="COCOON", dong="Gel rửa mặt bí đao", dung_tich="140 ml",
         nen=("#eef6e8", "#d6ead0"), mau_than="#8cbf6e", mau_nap="#3f6b2f", mau_nhan="#f7fbf2", mau_chu="#2f5223",
         ket_cau="gel", mau_ket_cau="#b9dca0", chu_thich="Gel trong mát, ít bọt", mau_chu_nen="#2f5223"),
    dict(tep="tinh-chat-niacinamide-10", hinh="nho_giot", thuong_hieu="SOME BY MI", dong="Niacinamide 10%", dung_tich="30 ml",
         nen=("#eef3fb", "#d9e4f5"), mau_than="#cfe0f5", mau_nap="#1f3b66", mau_nhan="#ffffff", mau_chu="#1f3b66"),
    dict(tep="kem-duong-phuc-hoi-b5", hinh="tuyp", thuong_hieu="LA ROCHE-POSAY", co_thuong_hieu=36, dong="Kem dưỡng phục hồi B5",
         dung_tich="40 ml", nen=("#f2f6fb", "#dde8f4"), mau_than="#ffffff", mau_nap="#2b6cb0", mau_nhan="#2b6cb0", mau_chu="#1d4f86"),
    dict(tep="kem-chong-nang-kiem-dau", hinh="tuyp", thuong_hieu="INNISFREE", dong="Chống nắng kiềm dầu SPF50+",
         dung_tich="50 ml", nen=("#eef7f0", "#d3eadb"), mau_than="#2f855a", mau_nap="#1c5638", mau_nhan="#c6f0d6", mau_chu="#ffffff"),
    # ---- 8 sản phẩm mới
    dict(tep="sua-rua-mat-senka-perfect-whip", hinh="tuyp", thuong_hieu="SENKA", dong="Perfect Whip – Sữa rửa mặt tạo bọt",
         dung_tich="120 g", nen=("#eef4fc", "#d6e5f8"), mau_than="#ffffff", mau_nap="#1f5fbf", mau_nhan="#1f5fbf", mau_chu="#174a96",
         ket_cau="bot", mau_ket_cau="#ffffff", chu_thich="Bọt mịn dày, làm sạch sâu", mau_chu_nen="#174a96"),
    dict(tep="sua-rua-mat-senka-perfect-whip-acne-care", hinh="tuyp", thuong_hieu="SENKA", dong="Perfect Whip Acne Care",
         dung_tich="100 g", nen=("#ecf8f5", "#d0eee6"), mau_than="#ffffff", mau_nap="#2a9d8f", mau_nhan="#2a9d8f", mau_chu="#1d6e64",
         ket_cau="bot", mau_ket_cau="#f4fffc", chu_thich="Bọt mịn cho da mụn", mau_chu_nen="#1d6e64"),
    dict(tep="gel-duong-am-senka-deep-moist", hinh="hu", thuong_hieu="SENKA", dong="Deep Moist Gel – Gel dưỡng ẩm",
         dung_tich="100 g", nen=("#edf5fd", "#d2e6fa"), mau_than="#bcdcf7", mau_nap="#3f86d4", mau_nhan="#ffffff", mau_chu="#225fa3",
         ket_cau="gel", mau_ket_cau="#a9d2f5", chu_thich="Gel mọng nước, thấm nhanh", mau_chu_nen="#225fa3"),
    dict(tep="gel-chong-nang-senka-perfect-uv", hinh="tuyp", thuong_hieu="SENKA", dong="Perfect UV Gel SPF50+ PA++++",
         dung_tich="80 g", nen=("#fdf7e8", "#f8e7bd"), mau_than="#ffffff", mau_nap="#e0a526", mau_nhan="#3b82c4", mau_chu="#22598f",
         ket_cau="gel", mau_ket_cau="#fff3c9", chu_thich="Gel chống nắng mỏng nhẹ", mau_chu_nen="#7a5a10"),
    dict(tep="kem-u-toc-fino-premium-touch", hinh="hu", thuong_hieu="FINO", dong="Premium Touch – Kem ủ tóc",
         dung_tich="230 g", nen=("#fbeeee", "#f2d4d4"), mau_than="#b8262e", mau_nap="#d8b15a", mau_nhan="#fff6ec", mau_chu="#8e1c23",
         ket_cau="kem", mau_ket_cau="#fff3e3", chu_thich="Kem ủ đặc, mềm mượt tóc", mau_chu_nen="#8e1c23"),
    dict(tep="dau-goi-tsubaki-premium-repair", hinh="chai_bom", thuong_hieu="TSUBAKI", dong="Premium Repair Shampoo",
         dung_tich="490 ml", nen=("#fbf5e6", "#f1e2bb"), mau_than="#c9a227", mau_nap="#8a6d14", mau_nhan="#fffaf0", mau_chu="#6e560f",
         ket_cau="bot", mau_ket_cau="#fffaf0", chu_thich="Bọt dày, phục hồi tóc hư tổn", mau_chu_nen="#6e560f"),
    dict(tep="dau-xa-tsubaki-premium-repair", hinh="chai_bom", thuong_hieu="TSUBAKI", dong="Premium Repair Conditioner",
         dung_tich="490 ml", nen=("#fbf6ec", "#efe3c8"), mau_than="#f3e6c4", mau_nap="#8a6d14", mau_nhan="#c9a227", mau_chu="#ffffff",
         ket_cau="kem", mau_ket_cau="#fff8ea", chu_thich="Kem xả mềm, gỡ rối dễ dàng", mau_chu_nen="#6e560f"),
    dict(tep="dau-goi-tsubaki-premium-moist", hinh="chai_bom", thuong_hieu="TSUBAKI", dong="Premium Moist Shampoo",
         dung_tich="490 ml", nen=("#fcedef", "#f3d0d5"), mau_than="#b3263a", mau_nap="#6e1422", mau_nhan="#fff4f5", mau_chu="#8a1c2c",
         ket_cau="bot", mau_ket_cau="#fff6f7", chu_thich="Dưỡng ẩm cho tóc khô xơ", mau_chu_nen="#8a1c2c"),
]


def luu(img, duong_dan):
    img.resize((W, H), Image.LANCZOS).convert("RGB").save(duong_dan, "WEBP", quality=88, method=6)


def anh_mac_dinh(dich):
    img = nen(("#f6f3f5", "#ebe5e9"), 7)
    d = ImageDraw.Draw(img)
    cx = W * S // 2
    d.rounded_rectangle([cx - 110 * S, 300 * S, cx + 110 * S, 640 * S], radius=50 * S, fill=hex2rgb("#d9cfd5"))
    d.rounded_rectangle([cx - 55 * S, 220 * S, cx + 55 * S, 310 * S], radius=16 * S, fill=hex2rgb("#c4b7bf"))
    d.text((cx, 720 * S), "Chưa có ảnh sản phẩm", font=f("segoeui.ttf", 34), fill=hex2rgb("#8a7c85"), anchor="mm")
    luu(img, dich / "mac-dinh.webp")


def main():
    dich = Path(sys.argv[1])
    dich.mkdir(parents=True, exist_ok=True)
    for i, sp in enumerate(SAN_PHAM):
        img = VE[sp["hinh"]](nen(sp["nen"], i), sp)
        luu(img, dich / f'{sp["tep"]}-1.webp')
        if "ket_cau" in sp:
            luu(ve_ket_cau(nen(sp["nen"], i + 100), sp), dich / f'{sp["tep"]}-2.webp')
    anh_mac_dinh(dich)
    for p in sorted(dich.glob("*.webp")):
        print(p.name, p.stat().st_size)


if __name__ == "__main__":
    main()
