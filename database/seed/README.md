# Dữ liệu mẫu bổ sung

Không sửa `database/ReviewIntelligence_SQLServer.sql` (schema gốc khớp báo cáo). Các script ở đây chỉ **thêm dữ liệu**.

| Tệp | Nội dung |
|---|---|
| `san_pham_bo_sung.sql` | Danh mục *Chăm sóc tóc* (Dầu gội, Dầu xả & kem ủ tóc), thương hiệu Senka/Fino/Tsubaki, 8 sản phẩm mới kèm `BienThe`, `LoHang`, `SanPham_LoaiDa`; gán ảnh cho 4 sản phẩm gốc. Chạy lại nhiều lần an toàn. |
| `tao_anh_san_pham_mau.py` | Vẽ ảnh minh họa WebP vào `frontend/public/images/san-pham/` (cần Pillow). Ảnh là hình vẽ minh họa, không phải ảnh chụp sản phẩm thật – thay bằng ảnh thật qua trang **Quản trị → Sản phẩm**. |

```powershell
sqlcmd -S localhost -d ReviewIntelligenceDB -E -f 65001 -i database/seed/san_pham_bo_sung.sql
# (tuỳ chọn) vẽ lại ảnh minh họa
python database/seed/tao_anh_san_pham_mau.py frontend/public/images/san-pham
```
