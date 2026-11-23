# Kết quả thực nghiệm ABSA

_Sinh tự động bởi `scripts/thuc_nghiem.py bao-cao` lúc 23/11/2026 17:47. Chỉ số chính đọc từ `dbo.vw_ChiSoThucNghiem`._

## Bộ dữ liệu

- Tập kiểm thử (`MauKiemThu.TapDuLieu = 'KiemThu'`): **47 mẫu** – Tích cực 16, Trung tính 15, Tiêu cực 16.
- Ví dụ few-shot (`TapDuLieu = 'ViDuMau'`): 9 mẫu – tách riêng, không trùng tập kiểm thử.

## Chỉ số theo cấu hình (cảm xúc tổng thể)

| Cấu hình | Số mẫu | Accuracy | Precision (macro) | Recall (macro) | F1 (macro) | Độ trễ TB | Trung vị | P95 | F1 khía cạnh (micro) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| PhoBERT (baseline) | 47 | 85.1% | 88.9% | 84.4% | 83.6% | 34 ms | 33 ms | 46 ms | 99.3% |
| LLM zero-shot | 2/47 | _chưa chạy đủ_ | | | | | | | |
| LLM few-shot | 2/47 | _chưa chạy đủ_ | | | | | | | |

- ⚠ **LLM zero-shot** mới có 2/47 mẫu (dữ liệu minh họa / lượt chạy dở) – chưa có số liệu so sánh. Chạy: `python -m scripts.thuc_nghiem chay LLM_KhongVD` (cần ANTHROPIC_API_KEY).
- ⚠ **LLM few-shot** mới có 2/47 mẫu (dữ liệu minh họa / lượt chạy dở) – chưa có số liệu so sánh. Chạy: `python -m scripts.thuc_nghiem chay LLM_ItVD` (cần ANTHROPIC_API_KEY).

Ghi chú: Precision/Recall/F1 macro = trung bình không trọng số trên 3 nhãn. F1 khía cạnh (micro) đo việc phát hiện đúng tập khía cạnh được nhắc tới (so `KhiaCanhDuDoan` với `KhiaCanhChuan`).

## PhoBERT (baseline) (`PhoBERT`)

### Chỉ số theo nhãn

| Nhãn | Precision | Recall | F1 | Số mẫu chuẩn |
|---|---:|---:|---:|---:|
| Tích cực | 72.7% | 100.0% | 84.2% | 16 |
| Trung tính | 100.0% | 53.3% | 69.6% | 15 |
| Tiêu cực | 94.1% | 100.0% | 97.0% | 16 |

### Ma trận nhầm lẫn (hàng = nhãn chuẩn, cột = dự đoán)

| Chuẩn \ Dự đoán | Tích cực | Trung tính | Tiêu cực |
|---|---:|---:|---:|
| Tích cực | **16** | 0 | 0 |
| Trung tính | 6 | **8** | 1 |
| Tiêu cực | 0 | 0 | **16** |

### Phân tích lỗi – 7/47 mẫu sai

| Nhóm lỗi | Số mẫu | Tỉ lệ trong mẫu sai | Chi tiết |
|---|---:|---:|---|
| Không nhận ra trung tính | 7 | 100.0% | Không nhận ra trung tính – đoán TichCuc (6); Không nhận ra trung tính – đoán TieuCuc (1) |

Ví dụ mẫu sai:

- #34 – “Sản phẩm dùng được, không có gì đặc biệt.” → chuẩn **Trung tính**, dự đoán **Tích cực** (Không nhận ra trung tính – đoán TichCuc)
- #38 – “Bao bì đơn giản, dùng ổn, chưa thấy kích ứng gì.” → chuẩn **Trung tính**, dự đoán **Tích cực** (Không nhận ra trung tính – đoán TichCuc)
- #43 – “Kết cấu đặc hơn mình nghĩ, tán kỹ một chút thì vẫn thấm.” → chuẩn **Trung tính**, dự đoán **Tích cực** (Không nhận ra trung tính – đoán TichCuc)
- #45 – “Ban đầu hơi châm chích nhẹ, dùng vài hôm thì hết, da cũng mịn hơn chút.” → chuẩn **Trung tính**, dự đoán **Tích cực** (Không nhận ra trung tính – đoán TichCuc)
- #46 – “Giao hàng nhanh, hộp còn nguyên seal, còn dùng thì để vài tuần nữa mới biết.” → chuẩn **Trung tính**, dự đoán **Tích cực** (Không nhận ra trung tính – đoán TichCuc)
- #47 – “Kiềm dầu được tầm 4 tiếng, sau đó phải dặm phấn lại.” → chuẩn **Trung tính**, dự đoán **Tiêu cực** (Không nhận ra trung tính – đoán TieuCuc)
- #48 – “Sản phẩm ổn so với giá tiền, không quá xuất sắc.” → chuẩn **Trung tính**, dự đoán **Tích cực** (Không nhận ra trung tính – đoán TichCuc)

## Hạn chế & lưu ý khi đưa vào báo cáo

- Bộ mẫu kiểm thử là dữ liệu **tổng hợp** (`ai-service/data/mau_kiem_thu.json`), gán nhãn thủ công; quy mô nhỏ (47 mẫu) nên mỗi mẫu ≈ 2.1 điểm phần trăm accuracy.
- Từ điển khía cạnh của PhoBERT được xây cùng thời điểm với bộ mẫu → F1 khía cạnh của PhoBERT là **ước lượng lạc quan**. Nên bổ sung mẫu từ đánh giá thật do người khác gán nhãn.
- Độ trễ PhoBERT đo trên GPU cục bộ; độ trễ LLM gồm thời gian mạng tới Claude API.
- Chỉ các cấu hình đã chạy đủ bộ kiểm thử mới được in số liệu; cấu hình thiếu mẫu (vd. vài dòng minh họa có sẵn trong schema) được đánh dấu “chưa chạy đủ”.
- PhoBERT được chạy khởi động 1 lần (không tính giờ) trước khi đo độ trễ.
