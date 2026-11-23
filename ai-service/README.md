# ai-service – Review Intelligence Agent

FastAPI phục vụ: phân tích cảm xúc theo khía cạnh (ABSA), vector nhúng + Qdrant, tóm tắt ưu/nhược điểm
(`TomTatKhiaCanh`), hỏi đáp RAG có dẫn chứng (LangGraph, ghi `TinNhan` + `NhatKyTacTu`) và thực nghiệm so sánh
mô hình (`MauKiemThu`, `KetQuaThucNghiem`, `vw_ChiSoThucNghiem`).

## Cài đặt (Windows)

```powershell
cd ai-service
py -3.12 -m venv .venv
.venv\Scripts\python -m pip install --upgrade pip
# PyTorch theo phần cứng: GPU NVIDIA (CUDA 12.4) hoặc CPU
.venv\Scripts\python -m pip install torch --index-url https://download.pytorch.org/whl/cu124
.venv\Scripts\python -m pip install -r requirements.txt
copy .env.example .env   # rồi sửa SQLSERVER_CONNECTION_STRING (cần ODBC Driver 18 for SQL Server)
```

Chạy:

```powershell
$env:PYTHONUTF8 = "1"
.venv\Scripts\python -m uvicorn app.main:app --host 127.0.0.1 --port 8000
```

Lần đầu tải mô hình từ Hugging Face: `wonrax/phobert-base-vietnamese-sentiment` (~540 MB) và `BAAI/bge-m3`
(~2.2 GB). Mô hình được nạp sẵn khi khởi động.

## Cấu hình mô hình & phương án dự phòng

| Thành phần | Mặc định | Khi thiếu |
|---|---|---|
| Qdrant | `QDRANT_MODE=local` – nhúng, lưu ở `qdrant_local/` | `docker compose up -d` + `QDRANT_MODE=server` |
| Embedding | BGE-M3 (GPU fp16 nếu có CUDA) | tự chuyển sang `hash` (truy hồi theo từ vựng, chất lượng thấp hơn) |
| ABSA `PhoBERT` | PhoBERT cảm xúc + từ điển khía cạnh (`app/services/van_ban.py`) | – |
| ABSA `LLM_KhongVD` / `LLM_ItVD` | Claude (`claude-opus-5-5`) zero-shot / few-shot (ví dụ = `MauKiemThu.TapDuLieu='ViDuMau'`) | **cần `ANTHROPIC_API_KEY`**; thiếu khóa thì cấu hình bị bỏ qua, không ghi kết quả |
| Sinh câu trả lời RAG | Claude (bắt buộc trích dẫn [n], kiểm tra sau sinh) | câu trả lời mẫu tổng hợp từ dẫn chứng + thống kê |

`MO_HINH_PHAN_TICH=auto`: dùng `LLM_ItVD` khi có khóa Claude, ngược lại `PhoBERT`. Giá trị `MoHinh` ghi vào DB luôn là
mô hình thực sự đã chạy.

## Endpoint

| Method | Path | Mô tả |
|---|---|---|
| GET | `/health` | SQL Server + Qdrant |
| POST | `/phan-tich/danh-gia` | `{"ma_danh_gia"}` – 202, phân tích nền đánh giá vừa duyệt |
| DELETE | `/phan-tich/danh-gia/{id}` | 202 – đánh giá bị ẩn: xóa vector, tính lại tóm tắt |
| POST | `/phan-tich/dong-bo?tat_ca=` | phân tích các đánh giá DaDuyet còn thiếu kết quả/vector |
| GET | `/phan-tich/mo-hinh?thu_khoi_tao=` | mô hình nào chạy được |
| POST | `/hoi-dap` | `{"cau_hoi","ma_san_pham","ma_nguoi_dung","ma_phien"}` – RAG có dẫn chứng |
| POST | `/thuc-nghiem/nap-mau` | nạp `data/mau_kiem_thu.json` vào `MauKiemThu` |
| POST | `/thuc-nghiem/chay` | `{"mo_hinh":[...]}` – 202, chạy nền |
| GET | `/thuc-nghiem/trang-thai` | tiến độ lượt chạy |

## Bộ mẫu kiểm thử

`data/mau_kiem_thu.json` là dữ liệu **tổng hợp** (tự soạn, gán nhãn thủ công): 45 mẫu `KiemThu` (15 mỗi nhãn) phủ
6 khía cạnh và 8 mẫu `ViDuMau` cho few-shot. Lưu ý: từ điển khía cạnh của cấu hình PhoBERT được xây dựng cùng thời
điểm với bộ mẫu, nên F1 phát hiện khía cạnh của PhoBERT trên bộ này là **ước lượng lạc quan**; nên bổ sung mẫu từ
đánh giá thật (do người khác gán nhãn) trước khi báo cáo số liệu cuối.
