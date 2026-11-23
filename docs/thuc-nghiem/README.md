# Thực nghiệm so sánh mô hình ABSA

Tài liệu phục vụ viết báo cáo và bảo vệ đồ án: thiết kế thực nghiệm, cách tái chạy và kết quả hiện có.
Số liệu chi tiết (tự sinh từ CSDL): [`ket-qua.md`](ket-qua.md).

## 1. Thiết kế

| Thành phần | Nội dung |
|---|---|
| Nhiệm vụ | Phân loại cảm xúc tổng thể của đánh giá (TichCuc / TrungTinh / TieuCuc) và phát hiện khía cạnh được nhắc tới (6 khía cạnh trong `dbo.KhiaCanh`) |
| Tập kiểm thử | `dbo.MauKiemThu` với `TapDuLieu = 'KiemThu'` – 47 mẫu (16 / 15 / 16) |
| Ví dụ few-shot | `TapDuLieu = 'ViDuMau'` – 9 mẫu, tách riêng khỏi tập kiểm thử |
| Cấu hình | **PhoBERT** (baseline): `wonrax/phobert-base-vietnamese-sentiment` + từ điển khía cạnh · **LLM_KhongVD**: Claude zero-shot · **LLM_ItVD**: Claude few-shot |
| Ghi kết quả | `dbo.KetQuaThucNghiem` – mỗi (MaMau, MoHinh) một dòng: `CamXucDuDoan`, `KhiaCanhDuDoan` (JSON), `LaDung`, `NhomLoi`, `DoTreMs`, `NgayChay` |
| Chỉ số | `dbo.vw_ChiSoThucNghiem`: Accuracy (`DoChinhXac`), Precision/Recall/F1 macro, `DoTreTrungBinhMs`. Bổ sung (backend + script): chỉ số theo nhãn, ma trận nhầm lẫn, trung vị/P95 độ trễ, F1 phát hiện khía cạnh (micro), nhóm lỗi |
| Nhóm lỗi | Gán tự động khi dự đoán sai (`ai-service/app/services/thuc_nghiem.py::nhom_loi`): *Câu tương phản (nhưng/tuy)*, *Không nhận ra trung tính*, *Đoán trung tính cho câu có cảm xúc*, *Đảo cực cảm xúc* |

## 2. Kết quả hiện có (23/11/2026)

| Cấu hình | Số mẫu | Accuracy | Precision (macro) | Recall (macro) | F1 (macro) | Độ trễ TB | Trung vị / P95 | F1 khía cạnh |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| PhoBERT | 47 | 85.1% | 88.9% | 84.4% | 83.6% | 34 ms | 33 / 46 ms | 99.3%* |
| LLM zero-shot | – | _chưa chạy – thiếu `ANTHROPIC_API_KEY`_ | | | | | | |
| LLM few-shot | – | _chưa chạy – thiếu `ANTHROPIC_API_KEY`_ | | | | | | |

\* Ước lượng lạc quan: từ điển khía cạnh và bộ mẫu do cùng một người soạn.

**Phân tích lỗi PhoBERT (7/47 mẫu sai):** 100% thuộc nhóm *Không nhận ra trung tính* (6 mẫu đoán Tích cực,
1 mẫu đoán Tiêu cực). Recall lớp Trung tính chỉ 53.3% trong khi Tích cực / Tiêu cực đạt 100%. Nguyên nhân: các
câu trung tính tiếng Việt thường chứa từ ngữ tích cực nhẹ ("dùng ổn", "da mịn hơn chút", "không quá xuất sắc")
mà mô hình cảm xúc 3 lớp nghiêng về Tích cực. Đây là động lực so sánh với LLM (hiểu ngữ cảnh tốt hơn) – giả thuyết
cần kiểm chứng khi chạy được hai cấu hình LLM.

> Hai dòng LLM trong `KetQuaThucNghiem` hiện có chỉ là **2 mẫu minh họa** đi kèm script schema, không phải kết quả
> chạy. Trang quản trị và báo cáo tự ẩn số liệu của cấu hình chưa chạy đủ bộ kiểm thử.

## 3. Cấu hình môi trường

`ai-service/.env` (mẫu: `ai-service/.env.example`):

| Biến | Ý nghĩa |
|---|---|
| `SQLSERVER_CONNECTION_STRING` | Kết nối `ReviewIntelligenceDB` (ODBC Driver 18) |
| `ANTHROPIC_API_KEY` | **Bắt buộc cho LLM_KhongVD / LLM_ItVD.** Để trống → hai cấu hình bị bỏ qua, không ghi kết quả |
| `LLM_MODEL` | Mặc định `claude-opus-5-5` |
| `LLM_EFFORT_PHAN_TICH` | Mức effort khi phân tích (mặc định `low`) |
| `DEVICE` | `auto` (dùng GPU CUDA nếu có) / `cpu` |

Chi phí LLM: 47 mẫu × 2 cấu hình = 94 lượt gọi ngắn (few-shot có prompt dài hơn) – tốn phí theo bảng giá Claude API.
Nếu khóa sai/hết hạn mức, cấu hình tự dừng sau 3 mẫu đầu lỗi liên tiếp.

> ⚠ **Chỉ chạy 1 tiến trình ai-service (uvicorn) cùng lúc** – Qdrant chạy chế độ nhúng và khóa
> `ai-service/qdrant_local/`. Script thực nghiệm bên dưới **không** mở Qdrant nên chạy được song song với ai-service,
> nhưng không bấm “Chạy” trên trang quản trị cùng lúc với lệnh `chay`.

## 4. Tái chạy

**Cách 1 – Trang quản trị** (`/admin/thuc-nghiem`, cần ai-service + backend): chọn cấu hình → *Chạy*. Trang hiển thị
bảng chỉ số, chỉ số theo nhãn, ma trận nhầm lẫn, nhóm lỗi kèm ví dụ và kết quả từng mẫu.

**Cách 2 – Dòng lệnh** (PowerShell, trong `ai-service`):

```powershell
$env:PYTHONUTF8 = "1"
.venv\Scripts\python -m scripts.thuc_nghiem kiem-tra --goi-thu     # xác thực khóa Claude (1 lượt gọi)
.venv\Scripts\python -m scripts.thuc_nghiem nap-mau                # nạp data/mau_kiem_thu.json (bỏ qua mẫu đã có)
.venv\Scripts\python -m scripts.thuc_nghiem chay PhoBERT LLM_KhongVD LLM_ItVD
.venv\Scripts\python -m scripts.thuc_nghiem bao-cao --ra ..\docs\thuc-nghiem\ket-qua.md
```

Truy vấn trực tiếp:

```sql
SELECT * FROM dbo.vw_ChiSoThucNghiem;
SELECT MoHinh, NhomLoi, COUNT(*) AS SoMau
FROM dbo.KetQuaThucNghiem kq JOIN dbo.MauKiemThu m ON m.MaMau = kq.MaMau AND m.TapDuLieu = 'KiemThu'
WHERE kq.LaDung = 0 GROUP BY MoHinh, NhomLoi ORDER BY MoHinh, SoMau DESC;
```

## 5. Hạn chế cần nêu trong báo cáo

- Bộ mẫu **tổng hợp, quy mô nhỏ** (47 mẫu → mỗi mẫu ≈ 2.1 điểm % accuracy); chưa có kiểm định ý nghĩa thống kê.
- F1 khía cạnh của PhoBERT lạc quan (từ điển và bộ mẫu cùng tác giả). Nên bổ sung mẫu từ đánh giá thật, do người
  khác gán nhãn, trước khi chốt số liệu.
- Độ trễ PhoBERT đo trên GPU RTX 3050 (đã khởi động trước khi đo); độ trễ LLM bao gồm thời gian mạng.
