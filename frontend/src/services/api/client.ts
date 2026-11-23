import type { NguoiDung } from '@/types/auth'
import type { ProblemDetails } from '@/types/api'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5080/api'
const TOKEN_KEY = 'ria_access_token'
const USER_KEY = 'ria_nguoi_dung'

/** Phát ra khi token hết hạn / bị từ chối (401) để AuthContext đăng xuất. */
export const SU_KIEN_HET_PHIEN = 'ria:het-phien'

export class ApiError extends Error {
  readonly status: number
  readonly problem?: ProblemDetails

  constructor(status: number, message: string, problem?: ProblemDetails) {
    super(message)
    this.status = status
    this.problem = problem
  }
}

function docLocal(key: string): string | null {
  try {
    return localStorage.getItem(key)
  } catch {
    return null
  }
}

function ghiLocal(key: string, value: string | null) {
  try {
    if (value === null) localStorage.removeItem(key)
    else localStorage.setItem(key, value)
  } catch {
    /* trình duyệt chặn storage – bỏ qua */
  }
}

export const tokenStorage = {
  get: () => docLocal(TOKEN_KEY),
  set: (token: string) => ghiLocal(TOKEN_KEY, token),
  clear: () => ghiLocal(TOKEN_KEY, null),
}

export const userStorage = {
  get: (): NguoiDung | null => {
    const raw = docLocal(USER_KEY)
    if (!raw) return null
    try {
      return JSON.parse(raw) as NguoiDung
    } catch {
      return null
    }
  },
  set: (nd: NguoiDung) => ghiLocal(USER_KEY, JSON.stringify(nd)),
  clear: () => ghiLocal(USER_KEY, null),
}

/** Gom thông báo lỗi từ ProblemDetails / ValidationProblemDetails của ASP.NET Core. */
function toMessage(problem: ProblemDetails | undefined, status: number): string {
  if (problem?.errors) {
    const first = Object.values(problem.errors).flat()[0]
    if (first) return first
  }
  if (problem?.title) return problem.title
  if (status === 401) return 'Vui lòng đăng nhập để tiếp tục.'
  if (status === 403) return 'Bạn không có quyền thực hiện thao tác này.'
  return `Lỗi ${status}`
}

/** Tạo query string, bỏ qua giá trị rỗng / undefined. */
export function taoQuery(params: object): string {
  const sp = new URLSearchParams()
  for (const [k, v] of Object.entries(params)) {
    if (v === undefined || v === null || v === '') continue
    sp.set(k, String(v))
  }
  const s = sp.toString()
  return s ? `?${s}` : ''
}

export async function request<T>(path: string, init: RequestInit = {}, baseUrl = API_BASE_URL): Promise<T> {
  const headers = new Headers(init.headers)
  // FormData: để trình duyệt tự đặt multipart/form-data kèm boundary.
  if (init.body && !(init.body instanceof FormData) && !headers.has('Content-Type'))
    headers.set('Content-Type', 'application/json')

  const token = tokenStorage.get()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const res = await fetch(`${baseUrl}${path}`, { ...init, headers })

  if (!res.ok) {
    const problem = (await res.json().catch(() => undefined)) as ProblemDetails | undefined
    if (res.status === 401 && token) {
      tokenStorage.clear()
      userStorage.clear()
      window.dispatchEvent(new Event(SU_KIEN_HET_PHIEN))
    }
    throw new ApiError(res.status, toMessage(problem, res.status), problem)
  }

  return res.status === 204 ? (undefined as T) : ((await res.json()) as T)
}

export const api = {
  get: <T>(path: string, signal?: AbortSignal) => request<T>(path, { signal }),
  post: <T>(path: string, body?: unknown) => request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(path: string, body?: unknown) => request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
  postForm: <T>(path: string, form: FormData) => request<T>(path, { method: 'POST', body: form }),
}
