/* =====================================================================================
   Dữ liệu mẫu bổ sung – sản phẩm chăm sóc da & tóc (Senka, Fino, Tsubaki)
   -------------------------------------------------------------------------------------
   - KHÔNG thay đổi cấu trúc bảng: chỉ thêm dữ liệu vào DanhMuc, ThuongHieu, SanPham,
     SanPham_LoaiDa, BienThe, LoHang và cập nhật SanPham.DanhSachAnh của 4 sản phẩm gốc.
   - Chạy lại nhiều lần an toàn (bỏ qua bản ghi đã có theo DuongDan / Ten / MaSKU / SoLo).
   - Ảnh nằm ở frontend/public/images/san-pham/ (ảnh minh họa, thay bằng ảnh thật qua /admin/san-pham).
   - BienThe.SoLuongTon = tổng LoHang.SoLuongConLai của các lô vừa nhập.
   Chạy:  sqlcmd -S localhost -d ReviewIntelligenceDB -E -f 65001 -i database/seed/san_pham_bo_sung.sql
   ===================================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ---------- Danh mục chăm sóc tóc ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc WHERE DuongDan = 'cham-soc-toc')
    INSERT dbo.DanhMuc(MaDanhMucCha, Ten, DuongDan) VALUES (NULL, N'Chăm sóc tóc', 'cham-soc-toc');

DECLARE @toc INT = (SELECT MaDanhMuc FROM dbo.DanhMuc WHERE DuongDan = 'cham-soc-toc');
IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc WHERE DuongDan = 'dau-goi')
    INSERT dbo.DanhMuc(MaDanhMucCha, Ten, DuongDan) VALUES (@toc, N'Dầu gội', 'dau-goi');
IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc WHERE DuongDan = 'dau-xa-kem-u')
    INSERT dbo.DanhMuc(MaDanhMucCha, Ten, DuongDan) VALUES (@toc, N'Dầu xả & kem ủ tóc', 'dau-xa-kem-u');

/* ---------- Thương hiệu ---------- */
INSERT dbo.ThuongHieu(Ten, QuocGia)
SELECT v.Ten, v.QuocGia
FROM (VALUES (N'Senka', N'Nhật Bản'), (N'Fino', N'Nhật Bản'), (N'Tsubaki', N'Nhật Bản')) v(Ten, QuocGia)
WHERE NOT EXISTS (SELECT 1 FROM dbo.ThuongHieu t WHERE t.Ten = v.Ten);

/* ---------- Ảnh cho 4 sản phẩm gốc (thay đường dẫn /anh/... chưa có tệp) ---------- */
UPDATE sp SET DanhSachAnh = v.Anh
FROM dbo.SanPham sp
JOIN (VALUES
    ('gel-rua-mat-bi-dao',       N'["/images/san-pham/gel-rua-mat-bi-dao-1.webp","/images/san-pham/gel-rua-mat-bi-dao-2.webp"]'),
    ('tinh-chat-niacinamide-10', N'["/images/san-pham/tinh-chat-niacinamide-10-1.webp"]'),
    ('kem-duong-phuc-hoi-b5',    N'["/images/san-pham/kem-duong-phuc-hoi-b5-1.webp"]'),
    ('kem-chong-nang-kiem-dau',  N'["/images/san-pham/kem-chong-nang-kiem-dau-1.webp"]')
) v(DuongDan, Anh) ON v.DuongDan = sp.DuongDan
WHERE sp.DanhSachAnh IS NULL OR sp.DanhSachAnh LIKE N'%"/anh/%';

/* ---------- Sản phẩm mới ---------- */
DECLARE @sp TABLE (
    DuongDan VARCHAR(280), DanhMuc VARCHAR(160), ThuongHieu NVARCHAR(150), Ten NVARCHAR(255),
    MoTa NVARCHAR(MAX), CongDung NVARCHAR(300), ThanhPhan NVARCHAR(MAX), HuongDan NVARCHAR(1000), Anh NVARCHAR(MAX));

INSERT @sp VALUES
('sua-rua-mat-senka-perfect-whip', 'sua-rua-mat', N'Senka',
 N'Sữa rửa mặt tạo bọt Senka Perfect Whip',
 N'Sữa rửa mặt dạng kem tạo bọt bán chạy hàng đầu. Chỉ cần một lượng nhỏ đã tạo được lớp bọt dày, mịn như kem tươi, len sâu vào lỗ chân lông để loại bỏ bụi bẩn và dầu thừa mà không gây khô căng.',
 N'Làm sạch sâu, tạo bọt mịn, giữ ẩm cho da',
 N'Glycerin, axit stearic, axit myristic, kali hydroxit, chiết xuất tơ tằm trắng (Sericin), Hyaluronic Acid.',
 N'Lấy khoảng 1 cm sữa rửa mặt, thêm chút nước và đánh bọt (dùng lưới tạo bọt càng tốt). Massage nhẹ nhàng lên mặt ướt rồi rửa sạch. Dùng sáng và tối.',
 N'["/images/san-pham/sua-rua-mat-senka-perfect-whip-1.webp","/images/san-pham/sua-rua-mat-senka-perfect-whip-2.webp"]'),

('sua-rua-mat-senka-perfect-whip-acne-care', 'sua-rua-mat', N'Senka',
 N'Sữa rửa mặt Senka Perfect Whip Acne Care cho da mụn',
 N'Phiên bản dành cho da dầu mụn: bọt mịn kết hợp hoạt chất kháng khuẩn giúp làm sạch nhờn, ngừa mụn ẩn và giảm bít tắc lỗ chân lông, vẫn dịu nhẹ khi dùng hằng ngày.',
 N'Làm sạch dầu nhờn, ngừa mụn, giảm bít tắc lỗ chân lông',
 N'Glycerin, axit lauric, Isopropyl Methylphenol, Dipotassium Glycyrrhizate, chiết xuất tơ tằm trắng.',
 N'Tạo bọt với nước, massage vùng chữ T và vùng da mụn khoảng 30 giây rồi rửa sạch. Tránh vùng mắt.',
 N'["/images/san-pham/sua-rua-mat-senka-perfect-whip-acne-care-1.webp","/images/san-pham/sua-rua-mat-senka-perfect-whip-acne-care-2.webp"]'),

('gel-duong-am-senka-deep-moist', 'kem-duong', N'Senka',
 N'Gel dưỡng ẩm Senka Deep Moist',
 N'Gel dưỡng mọng nước, thấm nhanh, không bết dính. Bổ sung độ ẩm tức thì và khóa ẩm suốt nhiều giờ, phù hợp khí hậu nóng ẩm và dùng làm lớp dưỡng trước khi trang điểm.',
 N'Cấp ẩm sâu, làm dịu da, không gây bết dính',
 N'Nước, Glycerin, Butylene Glycol, Hyaluronic Acid, Collagen thủy phân, chiết xuất tơ tằm trắng.',
 N'Sau bước làm sạch và toner, lấy lượng gel bằng hạt đậu thoa đều lên mặt và cổ, vỗ nhẹ cho thấm. Dùng sáng và tối.',
 N'["/images/san-pham/gel-duong-am-senka-deep-moist-1.webp","/images/san-pham/gel-duong-am-senka-deep-moist-2.webp"]'),

('gel-chong-nang-senka-perfect-uv', 'kem-chong-nang', N'Senka',
 N'Gel chống nắng Senka Perfect UV SPF50+ PA++++',
 N'Gel chống nắng kết cấu mỏng nhẹ như nước, chống tia UVA/UVB mạnh mẽ, không để lại vệt trắng, kiềm dầu tốt và có thể dùng làm lớp lót trang điểm.',
 N'Chống nắng SPF50+ PA++++, mỏng nhẹ, không vệt trắng',
 N'Nước, Ethylhexyl Methoxycinnamate, Bis-Ethylhexyloxyphenol Methoxyphenyl Triazine, Glycerin, Hyaluronic Acid.',
 N'Thoa đều lên mặt ở bước cuối chu trình dưỡng da buổi sáng, 15 phút trước khi ra nắng. Thoa lại sau mỗi 2–3 giờ hoặc sau khi ra mồ hôi nhiều.',
 N'["/images/san-pham/gel-chong-nang-senka-perfect-uv-1.webp","/images/san-pham/gel-chong-nang-senka-perfect-uv-2.webp"]'),

('kem-u-toc-fino-premium-touch', 'dau-xa-kem-u', N'Fino',
 N'Kem ủ tóc Fino Premium Touch',
 N'Kem ủ tóc cao cấp chứa 7 thành phần dưỡng giúp phục hồi tóc hư tổn do uốn, nhuộm, sấy nhiệt. Tóc mềm mượt, bóng khỏe và vào nếp ngay sau một lần sử dụng.',
 N'Phục hồi tóc hư tổn, mềm mượt, giảm xơ rối',
 N'Nước, Cetearyl Alcohol, Dimethicone, sữa ong chúa, tinh chất Royal Jelly EX, Squalane, dầu Argan.',
 N'Sau khi gội, vắt bớt nước, thoa kem lên thân và ngọn tóc (tránh da đầu), ủ 3–5 phút rồi xả sạch. Dùng 2–3 lần mỗi tuần.',
 N'["/images/san-pham/kem-u-toc-fino-premium-touch-1.webp","/images/san-pham/kem-u-toc-fino-premium-touch-2.webp"]'),

('dau-goi-tsubaki-premium-repair', 'dau-goi', N'Tsubaki',
 N'Dầu gội phục hồi Tsubaki Premium Repair',
 N'Dầu gội phục hồi hư tổn chuyên sâu với dầu hoa trà Tsubaki và Amino Acid, giúp làm sạch dịu nhẹ, nuôi dưỡng từ gốc đến ngọn, giảm gãy rụng và chẻ ngọn.',
 N'Làm sạch dịu nhẹ, phục hồi tóc hư tổn, giảm gãy rụng',
 N'Nước, Sodium Laureth Sulfate, Cocamidopropyl Betaine, dầu hạt hoa trà (Camellia Japonica Seed Oil), Amino Acid, Glycerin.',
 N'Làm ướt tóc, lấy lượng dầu gội vừa đủ tạo bọt và massage da đầu khoảng 1 phút, xả sạch. Kết hợp dầu xả cùng dòng để đạt hiệu quả tốt nhất.',
 N'["/images/san-pham/dau-goi-tsubaki-premium-repair-1.webp","/images/san-pham/dau-goi-tsubaki-premium-repair-2.webp"]'),

('dau-xa-tsubaki-premium-repair', 'dau-xa-kem-u', N'Tsubaki',
 N'Dầu xả phục hồi Tsubaki Premium Repair',
 N'Dầu xả dưỡng tóc phục hồi hư tổn, bổ sung dưỡng chất giúp tóc chắc khỏe, mềm mại, dễ chải và hạn chế rối, giữ nếp tóc suôn mượt cả ngày.',
 N'Dưỡng mềm, gỡ rối, phục hồi tóc hư tổn',
 N'Nước, Stearyl Alcohol, Behentrimonium Chloride, dầu hạt hoa trà, Amino Acid, Panthenol.',
 N'Sau khi gội, thoa dầu xả lên thân và ngọn tóc, để 1–2 phút rồi xả sạch với nước.',
 N'["/images/san-pham/dau-xa-tsubaki-premium-repair-1.webp","/images/san-pham/dau-xa-tsubaki-premium-repair-2.webp"]'),

('dau-goi-tsubaki-premium-moist', 'dau-goi', N'Tsubaki',
 N'Dầu gội dưỡng ẩm Tsubaki Premium Moist',
 N'Dầu gội dành cho tóc khô xơ, thiếu sức sống. Công thức giàu dưỡng ẩm giúp tóc mềm mượt, bóng khỏe và có hương hoa thanh lịch lưu lại lâu.',
 N'Dưỡng ẩm cho tóc khô xơ, tóc bóng mượt',
 N'Nước, Sodium Laureth Sulfate, dầu hạt hoa trà, mật ong, Hyaluronic Acid, Glycerin.',
 N'Làm ướt tóc, tạo bọt với lượng dầu gội vừa đủ, massage nhẹ nhàng rồi xả sạch.',
 N'["/images/san-pham/dau-goi-tsubaki-premium-moist-1.webp","/images/san-pham/dau-goi-tsubaki-premium-moist-2.webp"]');

INSERT dbo.SanPham(MaDanhMuc, MaThuongHieu, Ten, DuongDan, MoTa, CongDung, ThanhPhan, HuongDanSuDung, DanhSachAnh, TrangThai)
SELECT dm.MaDanhMuc, th.MaThuongHieu, s.Ten, s.DuongDan, s.MoTa, s.CongDung, s.ThanhPhan, s.HuongDan, s.Anh, 'DangBan'
FROM @sp s
JOIN dbo.DanhMuc dm ON dm.DuongDan = s.DanhMuc
JOIN dbo.ThuongHieu th ON th.Ten = s.ThuongHieu
WHERE NOT EXISTS (SELECT 1 FROM dbo.SanPham x WHERE x.DuongDan = s.DuongDan);

/* ---------- Loại da phù hợp ---------- */
INSERT dbo.SanPham_LoaiDa(MaSanPham, MaLoaiDa)
SELECT sp.MaSanPham, ld.MaLoaiDa
FROM (VALUES
    ('sua-rua-mat-senka-perfect-whip',           N'Da dầu'), ('sua-rua-mat-senka-perfect-whip', N'Da hỗn hợp'), ('sua-rua-mat-senka-perfect-whip', N'Da thường'),
    ('sua-rua-mat-senka-perfect-whip-acne-care', N'Da dầu'), ('sua-rua-mat-senka-perfect-whip-acne-care', N'Da hỗn hợp'),
    ('gel-duong-am-senka-deep-moist',            N'Da khô'), ('gel-duong-am-senka-deep-moist', N'Da hỗn hợp'), ('gel-duong-am-senka-deep-moist', N'Da thường'),
    ('gel-chong-nang-senka-perfect-uv',          N'Da dầu'), ('gel-chong-nang-senka-perfect-uv', N'Da hỗn hợp'), ('gel-chong-nang-senka-perfect-uv', N'Da thường'),
    ('kem-u-toc-fino-premium-touch',             N'Da khô'), ('kem-u-toc-fino-premium-touch', N'Da thường'),
    ('dau-goi-tsubaki-premium-repair',           N'Da dầu'), ('dau-goi-tsubaki-premium-repair', N'Da thường'),
    ('dau-xa-tsubaki-premium-repair',            N'Da thường'), ('dau-xa-tsubaki-premium-repair', N'Da khô'),
    ('dau-goi-tsubaki-premium-moist',            N'Da khô'), ('dau-goi-tsubaki-premium-moist', N'Da nhạy cảm'), ('dau-goi-tsubaki-premium-moist', N'Da thường')
) v(DuongDan, LoaiDa)
JOIN dbo.SanPham sp ON sp.DuongDan = v.DuongDan
JOIN dbo.LoaiDa ld ON ld.Ten = v.LoaiDa
WHERE NOT EXISTS (SELECT 1 FROM dbo.SanPham_LoaiDa x WHERE x.MaSanPham = sp.MaSanPham AND x.MaLoaiDa = ld.MaLoaiDa);

/* ---------- Biến thể (giá VND; khuyến mãi theo giờ UTC) ---------- */
DECLARE @btMoi TABLE (MaBienThe INT, MaSKU VARCHAR(50));

INSERT dbo.BienThe(MaSanPham, MaSKU, DungTich, GiaBan, GiaKhuyenMai, KhuyenMaiTu, KhuyenMaiDen, SoLuongTon)
OUTPUT inserted.MaBienThe, inserted.MaSKU INTO @btMoi
SELECT sp.MaSanPham, v.MaSKU, v.DungTich, v.GiaBan, v.GiaKhuyenMai, v.Tu, v.Den, 0
FROM (VALUES
    ('sua-rua-mat-senka-perfect-whip',           'SEN-WHIP-120',   N'120g',  109000, 89000,  CAST('2026-11-20' AS DATETIME2(0)), CAST('2026-12-15 23:59:59' AS DATETIME2(0))),
    ('sua-rua-mat-senka-perfect-whip',           'SEN-WHIP-50',    N'50g',    65000, NULL,   NULL, NULL),
    ('sua-rua-mat-senka-perfect-whip-acne-care', 'SEN-WHIPAC-100', N'100g',  115000, NULL,   NULL, NULL),
    ('gel-duong-am-senka-deep-moist',            'SEN-DMGEL-100',  N'100g',  199000, 169000, CAST('2026-11-20' AS DATETIME2(0)), CAST('2026-12-15 23:59:59' AS DATETIME2(0))),
    ('gel-chong-nang-senka-perfect-uv',          'SEN-UVGEL-80',   N'80g',   189000, NULL,   NULL, NULL),
    ('gel-chong-nang-senka-perfect-uv',          'SEN-UVGEL-40',   N'40g',   119000, NULL,   NULL, NULL),
    ('kem-u-toc-fino-premium-touch',             'FIN-MASK-230',   N'230g',  219000, 185000, CAST('2026-11-20' AS DATETIME2(0)), CAST('2026-12-31 23:59:59' AS DATETIME2(0))),
    ('dau-goi-tsubaki-premium-repair',           'TSB-RPSH-490',   N'490ml', 245000, NULL,   NULL, NULL),
    ('dau-goi-tsubaki-premium-repair',           'TSB-RPSH-330',   N'330ml', 189000, NULL,   NULL, NULL),
    ('dau-xa-tsubaki-premium-repair',            'TSB-RPCD-490',   N'490ml', 245000, 209000, CAST('2026-11-20' AS DATETIME2(0)), CAST('2026-12-15 23:59:59' AS DATETIME2(0))),
    ('dau-goi-tsubaki-premium-moist',            'TSB-MSSH-490',   N'490ml', 245000, NULL,   NULL, NULL)
) v(DuongDan, MaSKU, DungTich, GiaBan, GiaKhuyenMai, Tu, Den)
JOIN dbo.SanPham sp ON sp.DuongDan = v.DuongDan
WHERE NOT EXISTS (SELECT 1 FROM dbo.BienThe x WHERE x.MaSKU = v.MaSKU);

/* ---------- Lô hàng (chỉ cho biến thể vừa thêm) + đồng bộ tồn kho ---------- */
INSERT dbo.LoHang(MaBienThe, SoLo, HanSuDung, SoLuongNhap, SoLuongConLai)
SELECT b.MaBienThe, v.SoLo, v.HanSuDung, v.SoLuong, v.SoLuong
FROM (VALUES
    ('SEN-WHIP-120',   'S2609', CAST('2028-09-30' AS DATE), 60),
    ('SEN-WHIP-120',   'S2611', CAST('2029-01-31' AS DATE), 90),
    ('SEN-WHIP-50',    'S2610', CAST('2028-10-31' AS DATE), 80),
    ('SEN-WHIPAC-100', 'S2608', CAST('2028-08-31' AS DATE), 45),
    ('SEN-DMGEL-100',  'S2607', CAST('2028-07-15' AS DATE), 40),
    ('SEN-UVGEL-80',   'S2606', CAST('2028-06-30' AS DATE), 70),
    ('SEN-UVGEL-40',   'S2610', CAST('2028-10-15' AS DATE), 6),
    ('FIN-MASK-230',   'F2605', CAST('2029-05-31' AS DATE), 55),
    ('TSB-RPSH-490',   'T2604', CAST('2029-04-30' AS DATE), 50),
    ('TSB-RPSH-490',   'T2610', CAST('2029-10-31' AS DATE), 30),
    ('TSB-RPSH-330',   'T2609', CAST('2029-09-30' AS DATE), 35),
    ('TSB-RPCD-490',   'T2604', CAST('2029-04-30' AS DATE), 48),
    ('TSB-MSSH-490',   'T2603', CAST('2029-03-31' AS DATE), 36)
) v(MaSKU, SoLo, HanSuDung, SoLuong)
JOIN @btMoi b ON b.MaSKU = v.MaSKU;

UPDATE bt SET SoLuongTon = ISNULL((SELECT SUM(l.SoLuongConLai) FROM dbo.LoHang l WHERE l.MaBienThe = bt.MaBienThe), 0)
FROM dbo.BienThe bt
JOIN @btMoi m ON m.MaBienThe = bt.MaBienThe;

COMMIT TRANSACTION;

SELECT sp.MaSanPham, sp.DuongDan, COUNT(bt.MaBienThe) AS SoBienThe, SUM(bt.SoLuongTon) AS TongTon
FROM dbo.SanPham sp LEFT JOIN dbo.BienThe bt ON bt.MaSanPham = sp.MaSanPham
GROUP BY sp.MaSanPham, sp.DuongDan
ORDER BY sp.MaSanPham;
