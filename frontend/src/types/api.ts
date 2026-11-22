/** ProblemDetails (RFC 7807) do ASP.NET Core trả về khi lỗi. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  /** Chỉ có ở ValidationProblemDetails (lỗi 400 validation). */
  errors?: Record<string, string[]>
}
