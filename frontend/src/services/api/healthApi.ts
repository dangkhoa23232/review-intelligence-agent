import { request } from './client'

const AI_SERVICE_URL = import.meta.env.VITE_AI_SERVICE_URL ?? 'http://localhost:8000'

export const healthApi = {
  backend: () => request<{ trangThai: string }>('/health'),
  aiService: () => request<{ trang_thai: string }>('/health', {}, AI_SERVICE_URL),
}
