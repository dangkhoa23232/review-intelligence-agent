# review-intelligence-agent

Website mỹ phẩm tích hợp AI Agent phân tích cảm xúc theo khía cạnh (ABSA), tóm tắt ưu/nhược điểm và hỏi đáp review có dẫn chứng (RAG).

| Dịch vụ | Thư mục | Cổng |
|---|---|---|
| AI Service (FastAPI, PhoBERT, BGE-M3, Qdrant nhúng) | `ai-service/` | http://localhost:8000 |
| Backend API (ASP.NET Core 8 + EF Core) | `backend/ReviewIntelligence.Api/` | http://localhost:5080 (Swagger: `/swagger`) |
| Frontend Web (React + Vite) | `frontend/` | http://localhost:5173 |

## Chạy trong VS Code

`Ctrl+Shift+P` → **Tasks: Run Task**:

| Task | Việc làm |
|---|---|
| **Chạy tất cả dịch vụ** (cũng là build mặc định `Ctrl+Shift+B`) | Bật cả 3 dịch vụ song song, mỗi dịch vụ một terminal riêng |
| **AI Service (8000)** | `ai-service\.venv\Scripts\python.exe -m uvicorn app.main:app --port 8000` (thư mục `ai-service`, đọc `.env`) |
| **Backend API (5080)** | `dotnet run --project backend/ReviewIntelligence.Api/ReviewIntelligence.Api.csproj --launch-profile http` |
| **Frontend Web (5173)** | `npm run dev` trong `frontend` |
| **Chạy Backend + Frontend (không AI)** | Khi không cần ai-service |

Tắt từng dịch vụ: **Tasks: Terminate Task** (chọn task hoặc *All Running Tasks*), hoặc nút thùng rác ở terminal.
Debug (F5): chọn cấu hình trong `.vscode/launch.json` – *AI Service (debug)*, *Backend API (debug)*, *Frontend (Chrome)* hoặc *Debug toàn bộ*.

> ⚠ **ai-service chỉ được chạy 1 tiến trình cùng lúc** – Qdrant nhúng khóa thư mục `ai-service/qdrant_local/`.
> Không vừa chạy task *AI Service (8000)* vừa debug *AI Service (debug)*, và không mở thêm uvicorn ở terminal khác.

Yêu cầu trước: đã tạo `ai-service/.venv` và `ai-service/.env` (xem `ai-service/README.md`),
`backend/ReviewIntelligence.Api/appsettings.Development.json` (mẫu: `appsettings.Development.example.json`),
và `npm install` trong `frontend`.
