# Dự án: Review Intelligence Agent – Website Mỹ Phẩm

## Tổng quan
Đồ án Thực tập Chuyên ngành (12 tuần) xây dựng website bán mỹ phẩm tích hợp AI Agent phân tích cảm xúc theo khía cạnh (ABSA), tóm tắt ưu/nhược điểm và hỏi đáp review có dẫn chứng (RAG).

## Kiến trúc & Công nghệ
- **Database chính:** Microsoft SQL Server (`ReviewIntelligenceDB` - 28 bảng, schema chi tiết nằm ở `database/ReviewIntelligence_SQLServer.sql`).
- **Vector Database:** Qdrant (lưu embedding của review, liên kết qua cột `DanhGia.MaVectorQdrant`).
- **Backend Web:** C# .NET 8 Web API (Entity Framework Core) hoặc Python FastAPI (cập nhật lại sau khi chốt stack).
- **Frontend:** React / Next.js.
- **AI / Agent Service:** Python (BGE-M3 embedding, PhoBERT / LLM ABSA, LangGraph RAG Agent).

## Quy tắc bắt buộc khi sinh code
1. Mọi entity, model, truy vấn SQL phải khớp chính xác tên bảng và tên cột tiếng Việt không dấu trong `database/ReviewIntelligence_SQLServer.sql`.
2. Không tự ý thêm/sửa bảng trong SQL Server nếu chưa được xác nhận.
3. Các API trả về lỗi rõ ràng, có validation dữ liệu đầu vào.
